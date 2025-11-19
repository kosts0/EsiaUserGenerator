using EsiaUserGenerator.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("status")]
public class RequestStatusController : ControllerBase
{
    private readonly IRequestStatusStore _store;

    
    public RequestStatusController(IRequestStatusStore store)
    {
        _store = store;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(string id)
    {
        var status = await _store.GetStatusAsync(id);
        return status is null ? NotFound() : Ok(status);
    }
}