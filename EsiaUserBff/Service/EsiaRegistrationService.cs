using System.Diagnostics;
using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;

namespace EsiaUserGenerator.Service;

public class EsiaRegistrationService : IEsiaRegistrationService
{
    public EsiaRegistrationService(HttpClient http, ILogger<EsiaRegistrationService> logger)
    {
        _http = http;
        _logger = logger;
    }
    private readonly HttpClient _http;
    private readonly ILogger<EsiaRegistrationService> _logger;
    public Task<CreateUserResult> CreateUserAsync(CreateUserRequest esiaUserInfo, CancellationToken ct)
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


        

        _logger.LogInformation("User created successfully with UserId={UserId}", result.UserId);

        return Task.FromResult(result);
    }

    private async Task PostInitDataAsync(CreateUserRequest esiaUserInfo, CancellationToken ct)
    {
        var result =  await _http.PostAsJsonAsync<PostInitData>(new Uri("/registration_api/registration/v1/user-data"),

            new PostInitData()
            {
                FirstName = esiaUserInfo.EsiaUserInfo.FirstName,
                LastName = esiaUserInfo.EsiaUserInfo.LastName,
                Phone = esiaUserInfo.EsiaAuthInfo.Phone.ToPhoneValue()
            }, ct);
        return;
    }
}