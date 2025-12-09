using System.Text.Json.Nodes;

namespace EsiaUserMfe.Dto;

public class StatusResponse
{
    public int Code { get; set; }
    public string CodeStatus { get; set; }
    public class StatusResponseData
    {
        public string CurrentStatus { get; set; }
        public JsonNode GeneratedData { get; set; }
    }
    public StatusResponseData Data { get; set; }
}