using EsiaUserMfe.Dto;

namespace EsiaUserMfe.Service.Interface;

public interface IApiClient
{
    public Task<IEnumerable<EsiaUser>> GetFreeEsiaUsers();
    public Task<CreateEsiaUserResponse> CreateEsiaUser(EsiaUserRequest requestData);
    public Task<StatusResponse> GetRequestStatus(Guid requestId);
}