using Newtonsoft.Json;

namespace EsiaUserGenerator.Dto.Request;

public class AuthorizationRequest
{
    [JsonProperty("login")]
    public string Login { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
}