using EsiaUserGenerator.Utils.JsonConverter;
using Newtonsoft.Json;

namespace EsiaUserGenerator.Dto.Request;

public class SetCodeRequest
{
    [JsonProperty("code")]
    public string? Code { get; set; }
}