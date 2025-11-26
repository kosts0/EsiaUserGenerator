using EsiaUserGenerator.Db.UoW;
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
    private IUnitOfWork  _unitOfWork;
    public UsersController(IEsiaRegistrationService esia, IRequestStatusStore requestStatusStore, ILogger<UsersController> logger,
        IBackgroundTaskQueue taskQueue,
        IUnitOfWork unitOfWork)
    {
        _esia = esia;
        _requestStatusStore = requestStatusStore;
        _logger = logger;
        _taskQueue = taskQueue;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("start-user-create")]
    public async Task<IActionResult> StartUserCreate([FromBody] CreateUserRequest req)
    {
        var requestId = Guid.NewGuid();
        await _requestStatusStore.SetStatusAsync(requestId.ToString(), "Started");

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
                await uow.CompleteAsync();
            }
        });
        return Ok(new
        {
            RequestId = requestId,
            Status = "Queued"
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        req.Data ??= new CreateUserData();
        req.Data.GenarateDefaultValues();
        req.Data.RequestId ??= Guid.NewGuid();
        try
        {
            var result = await _esia.CreateUserAsync(req.Data, ct);
            await _requestStatusStore.SetStatusAsync(req.Data.RequestId.ToString(), "Created");
            return Ok(result);
        }
        catch (EsiaRequestException exception)
        {
            await _requestStatusStore.SetStatusAsync(req.Data.RequestId.ToString(),
                $"EsiaRequestException: {exception.Message}");
            return BadRequest(new CreateUserResult()
            {
                CodeStatus = "Esia integration error",
                ExceptionMessage = exception.Message,
                Code = 400,
                Exception = exception
            });
        }
        catch (System.Exception ex)
        {
            await _requestStatusStore.SetStatusAsync(req.Data.RequestId.ToString(), $"Exception: {ex.Message}");
            return BadRequest(new CreateUserResult()
            {
                CodeStatus = "Internal error",
                Code = 500,
                ExceptionMessage = ex.Message,
                Exception = ex
            });
        }
        finally
        {
            await _unitOfWork.CompleteAsync();
        }
    }
}