using EsiaUserGenerator.Dto.Model;

namespace EsiaUserGenerator.Dto;

public class CreateUserData
{
    public EsiaUserInfo? EsiaUserInfo { get; set; }
    public EsiaAuthInfo?  EsiaAuthInfo { get; set; }
}