using EsiaUserGenerator.Db.UoW;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("/api/user")]
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
        var result = await _unitOfWork.Users.GetAllLazyAsync();
        
        return Ok(result);
    }
    [HttpGet("/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _unitOfWork.Users.GetByIdAsync(id);
        return Ok(result);
    }
}