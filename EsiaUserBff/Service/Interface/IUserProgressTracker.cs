using EsiaUserGenerator.Dto.Enum;

namespace EsiaUserGenerator.Service.Interface;

public interface IUserProgressTracker
{
    Task SetStepAsync(Guid requestId, UserCreationFlow step);
}