using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Dto.API;

namespace EsiaUserGenerator.Dto;

public class CreateUserResult : ResponceBase<CreateUserResponseData>
{
}

public class CreateUserResponseData
{
    public EsiaUser EsiaUser { get; set; }
    
}