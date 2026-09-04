using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IMatchLifecycleService
{
    /// RG-ETA-002/003
    Task<DailyBatchResultDto> ExecuteDailyBatchAsync(DateOnly? today = null);
}
