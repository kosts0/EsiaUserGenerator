using System.Diagnostics;
using System.Text.RegularExpressions;
using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;
using EsiaUserGenerator.Dto.Request;
using EsiaUserGenerator.Exception;
using EsiaUserGenerator.Logs;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;
using Newtonsoft.Json.Linq;
using System;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;

namespace EsiaUserGenerator.Service;

public class EsiaRegistrationService : IEsiaRegistrationService
{
    private IRequestStatusStore _requestStatusStore;
    private ILoggerFactory _loggerFactory;
    private IUnitOfWork _unitOfWork;
    public EsiaRegistrationService(ILogger<EsiaRegistrationService> logger,  ILoggerFactory loggerFactory, 
        IRequestStatusStore requestStatusStore, 
        IUnitOfWork uow)
    { 
        _logger = logger;
        var loggerHandlerLogger = loggerFactory.CreateLogger<LoggingHandler>();
        _loggingHandler = new LoggingHandler(loggerHandlerLogger);
        _http = CreateClient();
        _requestStatusStore = requestStatusStore;
        _unitOfWork = uow;
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
        var requestId = esiaUserInfo.RequestId?.ToString() ?? Guid.NewGuid().ToString() ;
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
        _logger.LogInformation("RequestId user creation: {requestId}", requestId);
        CreateUserResult result = new CreateUserResult()
        {
        };
        EsiaUser esiaUserDb = new(){ 
            Id = Guid.NewGuid(),
            Login = esiaUserInfo.EsiaAuthInfo.Phone,
            Password = esiaUserInfo.EsiaAuthInfo.Password,
            DateTimeCreated = DateTime.Now
        };
        await _unitOfWork.Users.AddAsync(esiaUserDb);
        esiaUserDb.Status = nameof(PostInitDataAsync);
        await _requestStatusStore.SetStatusAsync(requestId, nameof(PostInitDataAsync));
        await PostInitDataAsync(esiaUserInfo, ct);
        

        esiaUserDb.Status = "Waiting sms";
        await _requestStatusStore.SetStatusAsync(requestId, $"Waiting sms. Phone: {esiaUserInfo.EsiaAuthInfo.Phone}");
        var sms = await RetryAsync.WhileNull(() => GetAuthSms(esiaUserInfo.EsiaAuthInfo.Phone),
            retries: 30, interval: TimeSpan.FromSeconds(5));
        
        esiaUserDb.Status = "Confirm SMS";
        await _requestStatusStore.SetStatusAsync(requestId, $"Confirm SMS");
        await SetCode(sms);

        esiaUserDb.Status = "Set password";
        await _requestStatusStore.SetStatusAsync(requestId, $"Set password");
        await CreatePassword(esiaUserInfo, ct);

        esiaUserDb.Status = "Oauth redirect ot authorization";
        await _requestStatusStore.SetStatusAsync(requestId, "Oauth redirect ot authorization");
        var oauth = await GetOauthEndpoint();
        
        
        await GoToOauth(oauth);
        var loginUrl = await Login(esiaUserInfo.EsiaAuthInfo, ct);
        
        esiaUserDb.Status = "Authorization";
        await _requestStatusStore.SetStatusAsync(requestId, "Authorization");
        await ExecuteRequest(() => _http.GetAsync(new Uri(loginUrl), ct));

        esiaUserDb.Status = "Update person data";
        await _requestStatusStore.SetStatusAsync(requestId, "Update person data");
        await UpdatePersonData(esiaUserInfo.EsiaUserInfo, ct);

        esiaUserDb.Status = nameof(SetPostmailConfirmation);
        await _requestStatusStore.SetStatusAsync(requestId, nameof(SetPostmailConfirmation));
        await SetPostmailConfirmation(ct);

        esiaUserDb.Status = "Wait postmail code";
        await _requestStatusStore.SetStatusAsync(requestId, "Wait postmail code");
        await GetPostCodes(ct);
        
        var postalCode = await RetryAsync.WhileNull(() =>
        {
            _logger.LogInformation($"Waiting for postal code for snils {esiaUserInfo.EsiaUserInfo.Snils}");
            return GetPostCode(esiaUserInfo.EsiaUserInfo.Snils, ct);
        }, retries: 30, interval: TimeSpan.FromSeconds(5));

        esiaUserDb.Status = "Confirm postmail code";
        await _requestStatusStore.SetStatusAsync(requestId, "Confirm postmail code");
        await ConfirmPostal(postalCode, ct);
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
        await ExecuteRequest(() =>  _http.PostAsJsonAsync<PostInitData>("registration_api/registration/v1/user-data",
            new PostInitData()
            {
                FirstName = esiaUserInfo?.EsiaUserInfo?.FirstName,
                LastName = esiaUserInfo?.EsiaUserInfo?.LastName,
                Phone = esiaUserInfo?.EsiaAuthInfo?.Phone
            }, ct));
        return;
    }

    private async Task<string?> GetAuthSms(string phone)
    {
        var smsList = await _http.GetAsync("/logs/sms/");
        var smsListContent = await smsList.Content.ReadAsStringAsync();
        string pattern = $@"Код:\s*(\d+)\s*to number\s*{phone.Replace("+", "\\+").Replace("(", "\\(").Replace(")","\\)")}";
        Match match = Regex.Match(smsListContent, pattern);

        if (match.Success)
        {
            string code = match.Groups[1].Value;
            _logger.LogInformation($"Код для {phone}: {code}");
            return code;
        }

        return null;
    }

    private async Task ExecuteRequest(Func<Task<HttpResponseMessage>> httpRequestAction)
    {
        var response = await httpRequestAction.Invoke();
        var content =  await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new EsiaRequestException($@"Error during execute request {response?.RequestMessage?.RequestUri};
Response: {content};
Status: {response.StatusCode}");
        }
    }
    
    public async Task SetCode(string code)
    {
        await ExecuteRequest(() => _http.PostAsJsonAsync<SetCodeRequest>("/registration_api/activation/phone/verify-code", new SetCodeRequest()
        {
            Code = code.ToString()
        }));
    }
    private async Task CreatePassword(CreateUserData esiaUserInfo, CancellationToken ct)
    {
        await ExecuteRequest(() => _http.PostAsJsonAsync("/registration_api/complete", new SetPassword()
        {
            Password = esiaUserInfo?.EsiaAuthInfo?.Password
        }, ct));
    }

    private async Task<string> GetOauthEndpoint()
    {
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Get, "/profile/login/"))
        {
            requestMessage.Headers.Add("Referer", _http.BaseAddress + "/profile/user/personal");
           var responseMessage = await _http.SendAsync(requestMessage);
           var locationHeader = responseMessage.Headers.Location;
           if (locationHeader == null) throw new System.Exception("Error during get oauth url");
           _logger.LogInformation("Response from location: {Location}", locationHeader);
           return locationHeader.ToString();
        }
        return null;
    }

    private async Task<string> Login(EsiaAuthInfo authInfo, CancellationToken ct)
    {
        var resonse = await _http.PostAsJsonAsync<AuthorizationRequest>("/aas/oauth2/api/login", new AuthorizationRequest()
        {
            Login = authInfo.Phone.Replace("(", "").Replace(")", ""),
            Password = authInfo.Password
        });
        if (!resonse.IsSuccessStatusCode) throw new EsiaRequestException("Error during authorization");
        var resposeContent = await resonse.Content.ReadAsStringAsync(ct);
        string? redirectUri = JToken.Parse(resposeContent)?.SelectToken("$..redirect_url")?.ToString();
        if (string.IsNullOrEmpty(redirectUri)) throw new EsiaRequestException("Error during get authorization redirect_uri");
        _logger.LogInformation("Redirect url after login request: {RedirectUri}", redirectUri);
        return redirectUri;
    }

    private async Task UpdatePersonData(EsiaUserInfo  esiaUserInfo, CancellationToken ct)
    {
        await ExecuteRequest(() => _http.PostAsJsonAsync<EsiaUserInfo>("/profile/rs/prns/up", esiaUserInfo, ct));
        return;
    }

    private async Task SetPostmailConfirmation(CancellationToken ct)
    {
        var requestBody = new
        {
            type = "PLV",
            countryId = "RUS",
            zipCode = 644123,
            fiasCode = "",
            region = "м",
            district = "москва",
            settlement = "москва",
            house = "1",
            flat = "1",
            addressStr = "м регион, москва доп. территория, москва доп. улица"
        };
        await ExecuteRequest(() => _http.PostAsJsonAsync("/profile/rs/prns/usrcfm/addr", requestBody, ct));
    }

    private async Task<string> GetPostCodes(CancellationToken ct)
    {
        var responce = await _http.GetAsync("/logs/postcodes/", ct);
        var responceCntent = await responce.Content.ReadAsStringAsync(ct);
        return responceCntent;
    }

    private async Task<string?> GetPostCode(string inn, CancellationToken ct)
    {
        var postCodes = await GetPostCodes(ct);
        string pattern = @$"Post code \({inn}\)\s*: (\d+)";
        Match match = Regex.Match(postCodes, pattern);

        if (match.Success)
        {
            string code = match.Groups[1].Value;
            _logger.LogInformation($"Код для {inn}: {code}");
            return code;
        }

        return null;
    }
    private async Task ConfirmPostal(string code, CancellationToken ct)
    {
        await ExecuteRequest(() => _http.GetAsync($"/profile/rs/prns/usrcfm/by-post?cfmPostCode={code}", ct));
    }

    private async Task GoToOauth(string oauth)
    {
        _logger.LogInformation("Go to oauth url: {OauthUrl}", oauth);

        var response =await _http.GetAsync(oauth);
        while (response.Headers.Location != null)
        {
            _logger.LogInformation("Go to redirect url: {OauthUrl}", response.Headers.Location);
            response =await _http.GetAsync(response.Headers.Location);
            var redirectHeader = response.Headers.Location;
            if (redirectHeader != null && redirectHeader.ToString().Contains("/user/personal"))
            {
                _logger.LogInformation("Finish redirect chain before {redirectHeader}", redirectHeader);
                break;
            }
            _logger.LogDebug("Headers after redirect : {headers}", response.Headers);
        }
    }
}