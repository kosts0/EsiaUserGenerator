using EsiaUserGenerator.Db.UoW;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("/api/storage")]
public class UserStorageController : ControllerBase
{
    private IUnitOfWork  _unitOfWork;

    public UserStorageController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    [HttpGet("commonUsers")]
    public async Task<IActionResult> GetFreeEsiaUsers()
    {
        var result = await _unitOfWork.Users.GetAllAsync();
        return Ok(result);
    }
}