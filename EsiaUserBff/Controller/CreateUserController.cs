using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.API;
using EsiaUserGenerator.Exception;
using EsiaUserGenerator.Service.Interface;
using EsiaUserGenerator.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EsiaUserGenerator.Controller;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IEsiaRegistrationService _esia;
    private readonly IRequestStatusStore _requestStatusStore;
    ILogger<UsersController> _logger;
    private IBackgroundTaskQueue _taskQueue;
    public UsersController(IEsiaRegistrationService esia, IRequestStatusStore requestStatusStore, ILogger<UsersController> logger, IBackgroundTaskQueue taskQueue)
    {
        _esia = esia;
        _requestStatusStore = requestStatusStore;
        _logger = logger;
        _taskQueue = taskQueue;
    }

    [HttpPost("start-user-create")]
    public async Task<IActionResult> StartUserCreate([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        Guid requestId = Guid.NewGuid();
        _logger.LogInformation("Start user creation process {requestId}", requestId);
        await _requestStatusStore.SetStatusAsync(requestId.ToString(), "Started");
        req.Data ??= new();
        req.Data.RequestId = requestId;
        await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
        {
            await Create(req, ct);
        });
        var response = new StartRequestResponse()
        {
            CodeStatus = "Created",
            Code = 201,
            Data = new StartRequestData()
            {
                RequestId = requestId
            }
        };
        return Ok(response);
    }
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        req.Data ??= new CreateUserData();
        req.Data.GenarateDefaultValues();
        try
        {
            var result = await _esia.CreateUserAsync(req.Data, ct);
            await _requestStatusStore.SetStatusAsync(req.Data.RequestId.ToString(), "Created");
            return Ok(result);
        }
        catch (EsiaRequestException exception)
        {
            await _requestStatusStore.SetStatusAsync(req.Data.RequestId.ToString(), $"EsiaRequestException: {exception.Message}");
            return BadRequest(new CreateUserResult()
            {
                CodeStatus = "Esia integration error",
                Exception = exception.Message,
                Code = 400
            });
        }
        catch (System.Exception ex)
        {
            await _requestStatusStore.SetStatusAsync(req.Data.RequestId.ToString(), $"Exception: {ex.Message}");
            return BadRequest(new CreateUserResult()
            {
                CodeStatus = "Internal error",
                Code = 500,
                Exception = ex.Message
            });
        }
    }
}