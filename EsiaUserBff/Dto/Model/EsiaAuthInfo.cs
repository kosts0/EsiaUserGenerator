using EsiaUserGenerator.Utils.JsonConverter;
using Newtonsoft.Json;

namespace EsiaUserGenerator.Dto.Model;

public class EsiaAuthInfo
{
    [JsonProperty("phone")]
    [JsonConverter(typeof(LongParseConverter))]
    public string?  Phone { get; set; }
    [JsonProperty("password")]
    public string? Password { get; set; }
}