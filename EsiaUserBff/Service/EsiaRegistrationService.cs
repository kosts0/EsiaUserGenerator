using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;
using EsiaUserGenerator.Exception;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;

public class EsiaRegistrationService : IEsiaRegistrationService
{
    private readonly IRequestStatusStore _requestStatusStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EsiaRegistrationService> _logger;


    public EsiaRegistrationService(
        ILogger<EsiaRegistrationService> logger,
        ILoggerFactory loggerFactory,
        IRequestStatusStore requestStatusStore,
        IUnitOfWork uow)
    {
        _logger = logger;
        _requestStatusStore = requestStatusStore;
        _unitOfWork = uow;



        _client = new RestClient("https://esia-portal1.test.gosuslugi.ru")
        {
            CookieContainer = _cookieContainer,
            
        };
    }
    private void SetRedirects(bool enabled)
    {
        _client.FollowRedirects = enabled;
    }

    private CookieContainer _cookieContainer = new CookieContainer();
    private RestClient _client;


    // --------------------------------------------------------
    // HIGH LEVEL API
    // --------------------------------------------------------

    public async Task<CreateUserResult> CreateUserAsync(CreateUserData esiaUserInfo, CancellationToken ct)
    {
        var requestId = esiaUserInfo.RequestId?.ToString() ?? Guid.NewGuid().ToString();
        _logger.LogInformation("Starting user creation {RequestId}", requestId);

        var esiaUserDb = new EsiaUser
        {
            Id = Guid.NewGuid(),
            Login = esiaUserInfo.EsiaAuthInfo.Phone,
            Password = esiaUserInfo.EsiaAuthInfo.Password,
            DateTimeCreated = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(esiaUserDb);
        await _requestStatusStore.SetStatusAsync(requestId, nameof(PostInitDataAsync));

        await PostInitDataAsync(esiaUserInfo, ct);

        await _requestStatusStore.SetStatusAsync(requestId, "Waiting sms");
        esiaUserDb.Status = "Waiting sms";

        var sms = await RetryAsync.WhileNull(
            () => GetAuthSms(esiaUserInfo.EsiaAuthInfo.Phone),
            retries: 30,
            interval: TimeSpan.FromSeconds(5)
        );

        esiaUserDb.Status = "Confirm SMS";
        await SetCode(sms);

        await CreatePassword(esiaUserInfo, ct);
       
        SetRedirects(false);
        var oauth = await GetOauthEndpoint();

        SetRedirects(true);
        await FollowAllRedirects(oauth);

        SetRedirects(false);
        var loginUrl = await Login(esiaUserInfo.EsiaAuthInfo, ct);

        SetRedirects(true);
        await Execute(() => new RestRequest(loginUrl, Method.GET));

        await UpdatePersonData(esiaUserInfo.EsiaUserInfo, ct);

        await SetPostmailConfirmation(ct);

        await GetPostCodes(ct);

        var postalCode = await RetryAsync.WhileNull(
            () => GetPostCode(esiaUserInfo.EsiaUserInfo.Snils, ct),
            retries: 30,
            interval: TimeSpan.FromSeconds(5)
        );

        await ConfirmPostal(postalCode, ct);

        return new CreateUserResult
        {
            Code = 201,
            CodeStatus = "Created",
            Data = new()
            {
                UserId = Guid.NewGuid()
            }
        };
    }

    // --------------------------------------------------------
    // REST METHODS
    // --------------------------------------------------------

    private async Task Execute(Func<IRestRequest> build)
    {
        var request = build();
        var response = await _client.ExecuteAsync(request);

        if (!response.IsSuccessful)
            throw new EsiaRequestException(
                $"Request failed: {request.Resource}\nStatus {response.StatusCode}\n{response.Content}"
            );
    }

    // ---------- POST INIT DATA ----------
    private async Task PostInitDataAsync(CreateUserData data, CancellationToken ct)
    {
        var request = new RestRequest("/registration_api/registration/v1/user-data", Method.POST)
            .AddJsonBody(new
            {
                first_name = data.EsiaUserInfo?.FirstName,
                last_name = data.EsiaUserInfo?.LastName,
                phone = data.EsiaAuthInfo?.Phone
            });

        await Execute(() => request);
    }

    // ---------- GET SMS ----------
    private async Task<string?> GetAuthSms(string phone)
    {
        var req = new RestRequest("/logs/sms/", Method.GET);
        var resp = await _client.ExecuteAsync(req);

        if (!resp.IsSuccessful) return null;

        string pattern = $@"Код:\s*(\d+)\s*to number\s*{Regex.Escape(phone)}";
        var match = Regex.Match(resp.Content ?? "", pattern);

        return match.Success ? match.Groups[1].Value : null;
    }

    // ---------- SET SMS CODE ----------
    public async Task SetCode(string code)
    {
        var req = new RestRequest("/registration_api/activation/phone/verify-code", Method.POST)
            .AddJsonBody(new { code = code });

        await Execute(() => req);
    }

    // ---------- SET PASSWORD ----------
    private async Task CreatePassword(CreateUserData data, CancellationToken ct)
    {
        var req = new RestRequest("/registration_api/complete", Method.POST)
            .AddJsonBody(new { password = data.EsiaAuthInfo?.Password });

        await Execute(() => req);
    }

    // ---------- OAUTH DISCOVERY ----------
    private async Task<string> GetOauthEndpoint()
    {
        var req = new RestRequest("/profile/login/", Method.GET)
            .AddHeader("Referer", "https://esia-portal1.test.gosuslugi.ru/profile/user/personal");
        var resp = await _client.ExecuteAsync(req);

        if (resp.Headers.FirstOrDefault(h => h.Name == "Location")?.Value is string url)
            return url;

        throw new Exception("OAuth endpoint not found");
    }

    // ---------- AUTH LOGIN ----------
    private async Task<string> Login(EsiaAuthInfo auth, CancellationToken ct)
    {
        var req = new RestRequest("/aas/oauth2/api/login", Method.POST)
            .AddJsonBody(new
            {
                login = auth.Phone.Replace("(", "").Replace(")", ""),
                password = auth.Password
            });

        var resp = await _client.ExecuteAsync(req);

        if (!resp.IsSuccessful)
            throw new EsiaRequestException("Auth failed");

        var redirect = JObject.Parse(resp.Content!)["redirect_url"]?.ToString();
        if (string.IsNullOrEmpty(redirect))
            throw new EsiaRequestException("Redirect URL not found");

        return redirect;
    }

    // ---------- FOLLOW REDIRECTS CHAIN ----------
    private async Task FollowAllRedirects(string url)
    {
        await Execute(() => new RestRequest(url));
        return;
        var next = url;

        while (true)
        {
            var req = new RestRequest(next, Method.GET);
            var resp = await _client.ExecuteAsync(req);

            if (resp.Headers.FirstOrDefault(h => h.Name == "Location")?.Value is not string location)
                break;

            if (location.Contains("/user/personal"))
                break;

            next = location;
        }
    }

    // ---------- PERSON UPDATE ----------
    private async Task UpdatePersonData(EsiaUserInfo info, CancellationToken ct)
    {
        var req = new RestRequest("/profile/rs/prns/up", Method.POST)
            .AddJsonBody(info);

        await Execute(() => req);
    }

    // ---------- SET POSTMAIL ----------
    private async Task SetPostmailConfirmation(CancellationToken ct)
    {
        var req = new RestRequest("/profile/rs/prns/usrcfm/addr", Method.POST)
            .AddJsonBody(new
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
            });

        await Execute(() => req);
    }

    // ---------- ALL POST CODES ----------
    private async Task<string> GetPostCodes(CancellationToken ct)
    {
        var resp = await _client.ExecuteAsync(new RestRequest("/logs/postcodes/", Method.GET));
        return resp.Content ?? "";
    }

    // ---------- GET POST CODE ----------
    private async Task<string?> GetPostCode(string inn, CancellationToken ct)
    {
        var content = await GetPostCodes(ct);
        var match = Regex.Match(content, @$"Post code \({inn}\)\s*: (\d+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    // ---------- CONFIRM POST ----------
    private async Task ConfirmPostal(string code, CancellationToken ct)
    {
        var req = new RestRequest($"/profile/rs/prns/usrcfm/by-post?cfmPostCode={code}", Method.GET);
        await Execute(() => req);
    }
}
