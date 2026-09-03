using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces.Repositories;
using Moq;

namespace BllTest.Services;

public class StatistiquesServiceTests
{
    private readonly Mock<IPaiementRepository> _paiementRepository = new();
    private readonly StatistiquesService _sut;

    public StatistiquesServiceTests()
    {
        _sut = new StatistiquesService(_paiementRepository.Object);
    }

    [Fact]
    public async Task CalculerChiffreAffairesAsync_SumsValidatedPayments()
    {
        int[] siteIds = [1, 2];
        var debut = new DateOnly(2026, 9, 1);
        var fin = new DateOnly(2026, 9, 30);
        _paiementRepository.Setup(r => r.GetValidatedBySitesAndPeriodAsync(siteIds, debut, fin)).ReturnsAsync(
        [
            new Paiement { MembreId = 1, Montant = 15m, Statut = StatutPaiement.Valide },
            new Paiement { MembreId = 2, Montant = 45m, Statut = StatutPaiement.Valide }
        ]);

        var result = await _sut.CalculerChiffreAffairesAsync(siteIds, debut, fin);

        Assert.Equal(60m, result.Montant);
        Assert.Equal(debut, result.Debut);
        Assert.Equal(fin, result.Fin);
    }

    [Fact]
    public async Task CalculerChiffreAffairesAsync_NoPayments_ReturnsZero()
    {
        _paiementRepository.Setup(r => r.GetValidatedBySitesAndPeriodAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync([]);

        var result = await _sut.CalculerChiffreAffairesAsync([1], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

        Assert.Equal(0m, result.Montant);
    }
}
