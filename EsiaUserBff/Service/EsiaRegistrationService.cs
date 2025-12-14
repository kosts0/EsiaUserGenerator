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
using System.Net;
using System.Security.Cryptography;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto.Enum;

namespace EsiaUserGenerator.Service;

public class EsiaRegistrationService : IEsiaRegistrationService
{
    private IRequestStatusStore _requestStatusStore;
    private ILoggerFactory _loggerFactory;
    private IUnitOfWork _unitOfWork;
    private IUserProgressTracker _userProgressTracker;
    public EsiaRegistrationService(ILogger<EsiaRegistrationService> logger,  ILoggerFactory loggerFactory, 
        IRequestStatusStore requestStatusStore, 
        IUnitOfWork uow,
        IUserProgressTracker  userProgressTracker)
    { 
        _logger = logger;
        _loggerFactory = loggerFactory;
        _http = CreateClient();
        _requestStatusStore = requestStatusStore;
        _unitOfWork = uow;
        _userProgressTracker = userProgressTracker;
        
    }
    //private CookieContainer cookieContainer = new CookieContainer();
    private HttpClientHandler _httpClientHandler;
    private HttpClient CreateClient()
    {
         _httpClientHandler = new HttpClientHandler()
        {
            UseCookies = true,
            AllowAutoRedirect = true,
            CookieContainer = new()
        };
        var loggerHandlerLogger = _loggerFactory.CreateLogger<LoggingHandler>();
        LoggingHandler loggingHandler = new(loggerHandlerLogger);
        loggingHandler.InnerHandler = _httpClientHandler;
        return  new HttpClient(loggingHandler)
        {
            BaseAddress = new Uri("https://esia-portal1.test.gosuslugi.ru")
        };
    }

    private bool mock = false;
    private HttpClient _http;
    private readonly ILogger<EsiaRegistrationService> _logger;
    public async Task<CreateUserResult> CreateUserAsync(CreateUserData esiaUserInfo, CancellationToken ct)
    {
        var requestId = esiaUserInfo.RequestId.ToString();
        esiaUserInfo.GenarateDefaultValues();
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
            CreatedRequestId = esiaUserInfo.RequestId,
            RequestData = await _unitOfWork.RequestHistory.GetByIdAsync(esiaUserInfo.RequestId)
        };
        if (esiaUserDb.RequestData != null) esiaUserDb.RequestData.GeneratedUserInfo = esiaUserInfo.EsiaUserInfo;
        await _unitOfWork.Users.AddAsync(esiaUserDb);
        //esiaUserDb.Status = nameof(PostInitDataAsync);
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.PostData);
        await _requestStatusStore.SetStatusAsync(requestId, nameof(PostInitDataAsync));
        await PostInitDataAsync(esiaUserInfo, ct);
        
        //esiaUserDb.Status = "Waiting sms";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.WaitingSms);
        await _requestStatusStore.SetStatusAsync(requestId, $"Waiting sms. Phone: {esiaUserInfo.EsiaAuthInfo.Phone}");
        var sms = await RetryAsync.WhileNull(() => GetAuthSms(esiaUserInfo.EsiaAuthInfo.Phone),
            retries: 40, interval: TimeSpan.FromSeconds(5));
        
        //esiaUserDb.Status = "Confirm SMS";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.ConfirmSms);
        await _requestStatusStore.SetStatusAsync(requestId, $"Confirm SMS");
        await SetCode(sms);

        //esiaUserDb.Status = "Set password";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.PasswordSet);
        await _requestStatusStore.SetStatusAsync(requestId, $"Set password");
        await CreatePassword(esiaUserInfo, ct);

        //esiaUserDb.Status = "Authorization";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.Authorization);
        await _requestStatusStore.SetStatusAsync(requestId, "Authorization");
        var oid = await Authorization(esiaUserInfo.EsiaAuthInfo, ct);
        esiaUserDb.Oid = oid;
        
        //esiaUserDb.Status = "Update person data";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.PersonDataUpdate);
        await _requestStatusStore.SetStatusAsync(requestId, "Update person data");
        
        await UpdatePersonData(esiaUserInfo, ct);

        //esiaUserDb.Status = nameof(SetPostmailConfirmation);
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.PostmailConfirmation);
        await _requestStatusStore.SetStatusAsync(requestId, nameof(SetPostmailConfirmation));
        await SetPostmailConfirmation(esiaUserInfo.EsiaAuthInfo,ct);

        //esiaUserDb.Status = "Wait postmail code";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.PostmailWaiting);
        await _requestStatusStore.SetStatusAsync(requestId, "Wait postmail code");
        await GetPostCodes(ct);
        
        var postalCode = await RetryAsync.WhileNull(() =>
        {
            _logger.LogInformation($"Waiting for postal code for snils {esiaUserInfo.EsiaUserInfo.Snils}");
            return GetPostCode(esiaUserInfo.EsiaUserInfo.Snils, ct);
        }, retries: 30, interval: TimeSpan.FromSeconds(5));

        //esiaUserDb.Status = "Confirm postmail code";
        //await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.PostmailConfirmation);
        await _requestStatusStore.SetStatusAsync(requestId, "Confirm postmail code");
        await ConfirmPostal(postalCode, ct);
        result.Data = new()
        {
            EsiaUser = esiaUserDb
        };
        result.CodeStatus = "Created";
        //esiaUserDb.Status = "Created";
        await _userProgressTracker.SetStepAsync(esiaUserInfo.RequestId, UserCreationFlow.Completed);
        
        _logger.LogInformation("User created successfully with UserId={UserId}", result.Data.EsiaUser.Id);
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

    private async Task ExecuteRequest(Func<Task<HttpResponseMessage>> httpRequestAction, Func<HttpResponseMessage,  bool>? retryCondition = null, Func<Task>? retryAction = null, int? retryCount = 5)
    {
        HttpResponseMessage response;
        response = await httpRequestAction.Invoke();
        while (retryCondition?.Invoke(response) == true && retryCount > 0)
        {
            _logger.LogWarning("Retrying request: {requestURL}", response?.RequestMessage?.RequestUri);
            await retryAction?.Invoke();
            response = await httpRequestAction.Invoke();
            retryCount--;
        }
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
           return null;
           
        }
        return null;
    }

    private async Task<string> Login(EsiaAuthInfo authInfo, CancellationToken ct)
    {
        var resonse = await _http.PostAsJsonAsync("/aas/oauth2/api/login", new
        {
            login = authInfo.Phone.Replace("(", "").Replace(")", ""),
            password = authInfo.Password
        }, cancellationToken: ct);
        if (!resonse.IsSuccessStatusCode) throw new EsiaRequestException("Error during authorization");
        var resposeContent = await resonse.Content.ReadAsStringAsync(ct);
        string? redirectUri = JToken.Parse(resposeContent)?.SelectToken("$..redirect_url")?.ToString();
        if (string.IsNullOrEmpty(redirectUri)) throw new EsiaRequestException("Error during get authorization redirect_uri");
        _logger.LogInformation("Redirect url after login request: {RedirectUri}", redirectUri);
        return redirectUri;
    }

    private async Task UpdatePersonData(CreateUserData  esiaUserInfo, CancellationToken ct)
    {
        await ExecuteRequest(async () => await _http.PostAsJsonAsync<EsiaUserInfo>("/profile/rs/prns/up", esiaUserInfo.EsiaUserInfo, ct),
            retryCondition: httpResponse => httpResponse.StatusCode == HttpStatusCode.Unauthorized, retryAction: async () => await Authorization(esiaUserInfo.EsiaAuthInfo, ct), retryCount:5 );
        return;
    }

    private async Task SetPostmailConfirmation(EsiaAuthInfo esiaAuthInfo,CancellationToken ct)
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
        await ExecuteRequest(() => _http.PostAsJsonAsync("/profile/rs/prns/usrcfm/addr", requestBody, ct),
            retryCondition: (response) => response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.Unauthorized,
            retryAction: () => Authorization(esiaAuthInfo,ct), 5);
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
        await ExecuteRequest(() => _http.PostAsync(new Uri($"/profile/rs/prns/usrcfm/by-post?cfmPostCode={code}", UriKind.Relative), null, ct));
    }
    

    private async Task<string?> AfterAuthRedirect(string loginUrl, CancellationToken ct)
    {
        await ExecuteRequest(() => _http.GetAsync(new Uri(loginUrl), ct));
        string oid = _httpClientHandler.CookieContainer.GetAllCookies().FirstOrDefault(c => c.Name=="oid")?.Value;
        return oid;
    }

    public async Task<string> Authorization(EsiaAuthInfo authInfo, CancellationToken ct)
    {
        string? oid;
        int retryCount = 5;
        do
        {
            _http = CreateClient();
            
            var oauth = await GetOauthEndpoint();

            var loginUrl = await Login(authInfo, ct);
           
            oid = await AfterAuthRedirect(loginUrl, ct);
            retryCount--;
        } while (string.IsNullOrEmpty(oid) && retryCount > 0);

        if (string.IsNullOrEmpty(oid) && retryCount == 0)
            throw new EsiaRequestException("Oid empty after authorization");
        return oid;
    }
}