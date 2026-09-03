using Core.Dtos;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Bll.Services;


public class StatistiquesService(IPaiementRepository paiementRepository) : IStatistiquesService
{
    public async Task<ChiffreAffairesDto> CalculerChiffreAffairesAsync(IEnumerable<int> siteIds, DateOnly debut, DateOnly fin)
    {
        var paiements = await paiementRepository.GetValidatedBySitesAndPeriodAsync(siteIds, debut, fin);
        decimal montant = paiements.Sum(p => p.Montant);
        return new ChiffreAffairesDto(montant, debut, fin);
    }
}
