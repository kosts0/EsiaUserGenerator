using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;

namespace EsiaUserGenerator.Service.Interface;

public interface IEsiaRegistrationService
{
    Task<CreateUserResult> CreateUserAsync(CreateUserData esiaUserInfo, CancellationToken ct);
}