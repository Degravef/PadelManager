using Core.Dtos;

namespace Core.Interfaces.Services;

public interface IStatistiquesService
{
    // RG-PAY-009 / CF-RC-004. Caller resolves which site ids it's allowed to see (e.g. an admin's own sites).
    Task<ChiffreAffairesDto> CalculerChiffreAffairesAsync(IEnumerable<int> siteIds, DateOnly debut, DateOnly fin);
}
