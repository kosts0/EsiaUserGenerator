using EsiaUserGenerator.Dto.API;

namespace EsiaUserGenerator.Dto;

public class CreateUserResult : ResponceBase<CreateUserResponseData>
{
}

public class CreateUserResponseData
{
    public Guid? UserId { get; set; }
}