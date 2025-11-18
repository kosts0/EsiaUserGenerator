using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IEsiaRegistrationService _esia;

    public UsersController(IEsiaRegistrationService esia)
    {
        _esia = esia;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        req.Data ??= new CreateUserData();
        req.Data.GenarateDefaultValues();
        try
        {
            var result = await _esia.CreateUserAsync(req.Data, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new CreateUserResult()
            {
                CodeStatus = "Internal error",
                Code = 500,
                Exception = ex.Message
            });
        }
    }
}