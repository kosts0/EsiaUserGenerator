using Newtonsoft.Json;

namespace EsiaUserGenerator.Dto.Request;

public class SetPassword
{
    [JsonProperty("password")]
    public string? Password { get; set; }
}