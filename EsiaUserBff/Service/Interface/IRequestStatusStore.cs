namespace EsiaUserGenerator.Service.Interface;


public interface IRequestStatusStore
{
    Task SetStatusAsync(string requestId, string status);
    Task<string?> GetStatusAsync(string requestId);
}