namespace EsiaUserGenerator.Dto;

public class CreateUserResult : ResponceBase<CreateUserResponseData>
{
}

public record CreateUserResponseData
{
    public Guid? UserId { get; set; }
}