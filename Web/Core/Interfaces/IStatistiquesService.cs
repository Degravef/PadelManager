using Web.Core.Dtos;

namespace Web.Core.Interfaces;

public interface IStatistiquesService
{
    Task<ChiffreAffairesDto> GetChiffreAffairesAsync(int? siteId, DateOnly debut, DateOnly fin);
}
