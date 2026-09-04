using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IMatchLifecycleService
{
    Task<DailyBatchResultDto> ExecuteDailyBatchAsync(DateOnly? today);
}
