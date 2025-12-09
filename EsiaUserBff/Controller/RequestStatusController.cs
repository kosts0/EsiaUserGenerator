using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto.API;
using EsiaUserGenerator.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("status")]
public class RequestStatusController : ControllerBase
{
    private readonly IRequestStatusStore _store;
    private readonly IUnitOfWork _unitOfWork;
    
    public RequestStatusController(IRequestStatusStore store,  IUnitOfWork unitOfWork)
    {
        _store = store;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var historyResult = await _unitOfWork.RequestHistory.GetByIdAsync(id);
        //var status = await _store.GetStatusAsync(id);


        if (historyResult == null)
        {
            return NotFound(new StatusResponse()
            {
                Code = 404,
                CodeStatus = "Status not found"
            });
        }

        return Ok(new 
        {
            Code = 200,
            CodeStatus = "Found",
            Data = new 
            {
                RequestId = historyResult.RequestId,
                RequestJsonData = historyResult.JsonRequest,
                CurrentStatus = historyResult.CurrentStatus,
                GeneratedData = historyResult.GeneratedUserInfo
            }
        });
    }
}