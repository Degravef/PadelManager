using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IMatchLifecycleService
{
    // RG-ETA-002/003: the J-1 daily batch (private->public switches, penalties, balances due).
    Task<DailyBatchResultDto> ExecuteDailyBatchAsync(DateOnly? today = null);
}
