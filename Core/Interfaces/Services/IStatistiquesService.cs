using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IStatistiquesService
{
    
    Task<ChiffreAffairesDto> CalculerChiffreAffairesAsync(IEnumerable<int> siteIds, DateOnly debut, DateOnly fin);
}
