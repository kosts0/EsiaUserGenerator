using System.Diagnostics;
using System.Text.RegularExpressions;
using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;
using EsiaUserGenerator.Dto.Request;
using EsiaUserGenerator.Logs;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;
using Newtonsoft.Json.Linq;

namespace EsiaUserGenerator.Service;

public class EsiaRegistrationService : IEsiaRegistrationService
{
    public EsiaRegistrationService(ILogger<EsiaRegistrationService> logger, LoggingHandler loggingHandler)
    { 
        _logger = logger;
        _loggingHandler = loggingHandler;
        _http = CreateClient();
    }
    private HttpClient CreateClient()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = true,
            AllowAutoRedirect = false
        };
        _loggingHandler.InnerHandler = handler;
        return new HttpClient(_loggingHandler)
        {
            BaseAddress = new Uri("https://esia-portal1.test.gosuslugi.ru")
        };
    }

    private readonly LoggingHandler _loggingHandler;
    private readonly HttpClient _http;
    private readonly ILogger<EsiaRegistrationService> _logger;
    public async Task<CreateUserResult> CreateUserAsync(CreateUserData esiaUserInfo, CancellationToken ct)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            _logger.LogInformation(
                "Starting user creation. TraceId={TraceId}, SpanId={SpanId}",
                activity.TraceId.ToHexString(),
                activity.SpanId.ToHexString());
        }
        else
        {
            _logger.LogWarning("Activity.Current is null — tracing not available.");
        }
        
        CreateUserResult result = new CreateUserResult()
        {
        };
        
        await PostInitDataAsync(esiaUserInfo, ct);

        var sms = await RetryAsync.WhileNull(() => GetAuthSms(esiaUserInfo.EsiaAuthInfo.Phone),
            retries: 30, interval: TimeSpan.FromSeconds(5));
        await SetCode(sms.Value);
        await CreatePassword(esiaUserInfo, ct);
        var oauth = await GetOauthEndpoint();
        await ExecuteRequest(() => _http.GetAsync(new Uri(oauth), ct));
        var loginUrl = await Login(esiaUserInfo.EsiaAuthInfo, ct);
        await ExecuteRequest(() => _http.GetAsync(new Uri(loginUrl), ct));
        
        result.Data = new()
        {
            UserId = Guid.NewGuid()
        };
        result.CodeStatus = "Created";
        
        _logger.LogInformation("User created successfully with UserId={UserId}", result.Data.UserId);

        return result;
    }
    
    private async Task PostInitDataAsync(CreateUserData esiaUserInfo, CancellationToken ct)
    {
        var result =  await _http.PostAsJsonAsync<PostInitData>("registration_api/registration/v1/user-data",
            new PostInitData()
            {
                FirstName = esiaUserInfo?.EsiaUserInfo?.FirstName,
                LastName = esiaUserInfo?.EsiaUserInfo?.LastName,
                Phone = esiaUserInfo?.EsiaAuthInfo?.Phone
            }, ct);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception(result.ToString());
        }
        return;
    }

    private async Task<long?> GetAuthSms(string phone)
    {
        var smsList = await _http.GetAsync("/logs/sms/");
        var smsListContent = await smsList.Content.ReadAsStringAsync();
        string pattern = $@"Код:\s*(\d+)\s*to number\s*{phone.Replace("+", "\\+").Replace("(", "\\(").Replace(")","\\)")}";
        Match match = Regex.Match(smsListContent, pattern);

        if (match.Success)
        {
            string code = match.Groups[1].Value;
            _logger.LogInformation($"Код для {phone}: {code}");
            return long.Parse(code);
        }

        return null;
    }

    private async Task ExecuteRequest(Func<Task<HttpResponseMessage>> httpRequestAction)
    {
        var response = await httpRequestAction.Invoke();
        var content =  await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error during execute request:\n {content}");
        }
    }
    
    public async Task SetCode(long code)
    {
        await ExecuteRequest(() => _http.PostAsJsonAsync<SetCodeRequest>("/registration_api/activation/phone/verify-code", new SetCodeRequest()
        {
            Code = code.ToString()
        }));
    }
    private async Task CreatePassword(CreateUserData esiaUserInfo, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync("/registration_api/complete", new SetPassword()
        {
            Password = esiaUserInfo?.EsiaAuthInfo?.Password
        }, ct);
        var content =  await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error during create password:\n {content}");
        }
    }

    private async Task<string> GetOauthEndpoint()
    {
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Get, "/profile/login/"))
        {
            requestMessage.Headers.Add("Referer", _http.BaseAddress + "/profile/user/personal");
           var responseMessage = await _http.SendAsync(requestMessage);
           var locationHeader = responseMessage.Headers.Location;
           if (locationHeader == null) throw new Exception("Error during get oauth url");
           _logger.LogInformation("Response from location: {Location}", locationHeader);
           return locationHeader.ToString();
        }
        return null;
    }

    private async Task<string> Login(EsiaAuthInfo authInfo, CancellationToken ct)
    {
        var resonse = await _http.PostAsJsonAsync<AuthorizationRequest>("/aas/oauth2/api/login", new AuthorizationRequest()
        {
            Login = authInfo.Phone,
            Password = authInfo.Password
        });
        if (!resonse.IsSuccessStatusCode) throw new Exception("Error during authorization");
        var resposeContent = await resonse.Content.ReadAsStringAsync(ct);
        string? redirectUri = JToken.Parse(resposeContent)?.SelectToken("$..redirect_uri")?.ToString();
        if (string.IsNullOrEmpty(redirectUri)) throw new Exception("Error during get authorization redirect_uri");
        return redirectUri;
    }
}