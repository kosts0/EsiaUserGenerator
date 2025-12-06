using System.Net;
using System.Net.Http.Json;
using EsiaUserMfe.Dto;
using EsiaUserMfe.Service.Interface;

namespace EsiaUserMfe.Service;

public class RestService : IApiClient
{
    private readonly HttpClient _httpClient;

    public RestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<IEnumerable<EsiaUser>> GetFreeEsiaUsers()
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<EsiaUser>>("/api/user/commonUsers");
        return result;
    }

    public async Task<CreateEsiaUserResponse> CreateEsiaUser(EsiaUserRequest requestData)
    {
        var result =
            await _httpClient.PostAsJsonAsync<EsiaUserRequest>(
                new Uri("/api/users/start-user-create", UriKind.Relative), requestData);
        return await result.Content.ReadFromJsonAsync<CreateEsiaUserResponse>();
    }

    public async Task<string> GetRequestStatus(Guid requestId)
    {
        var response = await _httpClient.GetAsync($"/status/{requestId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}