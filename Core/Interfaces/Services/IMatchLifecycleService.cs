using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IMatchLifecycleService
{
    
    Task<TraitementQuotidienResultDto> ExecuterTraitementQuotidienAsync(DateOnly? aujourdHui = null);
}
