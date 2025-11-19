using System.Text.Json.Serialization;
using EsiaUserGenerator.Dto.Model;

namespace EsiaUserGenerator.Dto;

public class CreateUserData
{
    [JsonIgnore]
    public Guid? RequestId { get; set; }
    public EsiaUserInfo? EsiaUserInfo { get; set; }
    public EsiaAuthInfo?  EsiaAuthInfo { get; set; }
}