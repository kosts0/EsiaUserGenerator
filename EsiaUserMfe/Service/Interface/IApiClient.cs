using EsiaUserMfe.Dto;

namespace EsiaUserMfe.Service.Interface;

public interface IApiClient
{
    public Task<IEnumerable<EsiaUser>> GetFreeEsiaUsers();
    public Task<CreateEsiaUserRespnonse> CreateEsiaUser(EsiaUserRequest requestData);
    public Task<string> GetRequestStatus(Guid requestId);
}