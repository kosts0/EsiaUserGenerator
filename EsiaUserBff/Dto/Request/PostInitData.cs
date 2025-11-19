using Newtonsoft.Json;

namespace EsiaUserGenerator.Dto.Model;

public class PostInitData
{
    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [JsonProperty("phone")]
    public string? Phone { get; set; }
}