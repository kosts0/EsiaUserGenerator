using Newtonsoft.Json;

namespace EsiaUserGenerator.Dto.API;

public class StartRequestResponse : ResponceBase<StartRequestData>
{
    
}

public class StartRequestData
{
    [JsonProperty("requestId")]
    public Guid RequestId { get; set; }
}