using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;

namespace EsiaUserGenerator.Service.Interface;

public interface IEsiaRegistrationService
{
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest esiaUserInfo, CancellationToken ct);
}