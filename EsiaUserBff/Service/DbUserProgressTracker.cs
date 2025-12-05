using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto.Enum;
using EsiaUserGenerator.Service.Interface;

namespace EsiaUserGenerator.Service;

public class DbUserProgressTracker : IUserProgressTracker
{
    private IUnitOfWork _uow;
    private ILogger<DbUserProgressTracker> _logger;
    public DbUserProgressTracker(IUnitOfWork unitOfWork,  ILogger<DbUserProgressTracker> logger)
    {
        _uow = unitOfWork;
        _logger = logger;
    }
    public async Task SetStepAsync(Guid requestId, UserCreationFlow step)
    {
        _logger.LogInformation($"Setting user progress {step} for {requestId}");
        try
        {
            var request = await _uow.RequestHistory.GetByIdAsync(requestId);
            if (request == null)
            {
                _logger.LogError($"Request entity {requestId} not found");
                return;
            }
            request.CurrentStatus = step.ToString();
            await _uow.CompleteAsync();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Failed to set status {userUd}", requestId);
            throw;
        }
    }
}