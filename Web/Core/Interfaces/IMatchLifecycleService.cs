using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IMatchLifecycleService
{
    Task<TraitementQuotidienResultDto> ExecuterAsync(DateOnly? date);
}
