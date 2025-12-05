using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.API;
using EsiaUserGenerator.Dto.Enum;
using EsiaUserGenerator.Exception;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("api/user")]
public sealed class UsersController : ControllerBase
{
    private readonly IEsiaRegistrationService _esia;
    private readonly IRequestStatusStore _requestStatusStore;
    ILogger<UsersController> _logger;
    private IBackgroundTaskQueue _taskQueue;
    private IUnitOfWork  _unitOfWork;
    private IUserProgressTracker _userProgressTracker;
    public UsersController(IEsiaRegistrationService esia, IRequestStatusStore requestStatusStore, ILogger<UsersController> logger,
        IBackgroundTaskQueue taskQueue,
        IUnitOfWork unitOfWork,
        IUserProgressTracker  userProgressTracker)
    {
        _esia = esia;
        _requestStatusStore = requestStatusStore;
        _logger = logger;
        _taskQueue = taskQueue;
        _unitOfWork = unitOfWork;
        _userProgressTracker = userProgressTracker;
    }

    [HttpPost("start-user-create")]
    public async Task<IActionResult> StartUserCreate([FromBody] CreateUserRequest req)
    {
        var requestId = Guid.NewGuid();
        await _unitOfWork.RequestHistory.AddAsync(entity: new RequestHistory()
        {
            RequestId = requestId,
            JsonRequest = JsonSerializer.Serialize(req),
            CurrentStatus = nameof(UserCreationFlow.Queued),
            DateTimeCreated = DateTime.UtcNow,
        });
        await _unitOfWork.CompleteAsync();
        await _userProgressTracker.SetStepAsync(requestId, UserCreationFlow.Queued);
        req.Data ??= new();
        req.Data.RequestId = requestId;

        await _taskQueue.QueueAsync(async (sp, ct) =>
        {
            var esiaService = sp.GetRequiredService<IEsiaRegistrationService>();
            var uow = sp.GetRequiredService<IUnitOfWork>();

            try
            {
                await esiaService.CreateUserAsync(req.Data, ct);
                await _requestStatusStore.SetStatusAsync(requestId.ToString(), "Completed");
            }
            catch (System.Exception ex)
            {
                await _requestStatusStore.SetStatusAsync(requestId.ToString(), "Error: " + ex.Message);
            }
            finally
            {
                (await uow.RequestHistory.GetByIdAsync(requestId)).Finished = true;
                await uow.CompleteAsync();
            }
        });
        return Ok(new
        {
            RequestId = requestId,
            Status = "Queued"
        });
    }
}