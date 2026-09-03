using Core.Domain.Entities;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class CreneauRepositoryTests
{
    private static async Task<HoraireSite> SeedHoraireAsync(PadelDbContext context, int siteAdminId = 1)
    {
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = siteAdminId };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var horaire = new HoraireSite { SiteId = site.Id, Annee = 2026 };
        context.HorairesSites.Add(horaire);
        await context.SaveChangesAsync();
        return horaire;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCreneau_ReturnsCreneau()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var horaire = await SeedHoraireAsync(context);
        var creneau = new Creneau { HoraireSiteId = horaire.Id, Ordre = 1, HeureDebut = new TimeOnly(9, 0), HeureFin = new TimeOnly(10, 45) };
        context.Creneaux.Add(creneau);
        await context.SaveChangesAsync();

        var sut = new CreneauRepository(context);
        var result = await sut.GetByIdAsync(creneau.Id);

        Assert.NotNull(result);
        Assert.Equal(1, result.Ordre);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new CreneauRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByHoraireSiteIdAsync_ReturnsOnlyMatchingHoraire_OrderedByOrdre()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var horaireA = await SeedHoraireAsync(context);
        var horaireB = await SeedHoraireAsync(context, siteAdminId: 2);
        context.Creneaux.AddRange(
            new Creneau { HoraireSiteId = horaireA.Id, Ordre = 2, HeureDebut = new TimeOnly(11, 0), HeureFin = new TimeOnly(12, 45) },
            new Creneau { HoraireSiteId = horaireA.Id, Ordre = 1, HeureDebut = new TimeOnly(9, 0), HeureFin = new TimeOnly(10, 45) },
            new Creneau { HoraireSiteId = horaireB.Id, Ordre = 1, HeureDebut = new TimeOnly(9, 0), HeureFin = new TimeOnly(10, 45) });
        await context.SaveChangesAsync();

        var sut = new CreneauRepository(context);
        var result = (await sut.GetByHoraireSiteIdAsync(horaireA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(horaireA.Id, c.HoraireSiteId));
        Assert.Equal([1, 2], result.Select(c => c.Ordre));
    }

    [Fact]
    public async Task GetByHoraireSiteIdAsync_NoCreneauxForHoraire_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new CreneauRepository(context);

        Assert.Empty(await sut.GetByHoraireSiteIdAsync(999));
    }

    [Fact]
    public async Task AddRangeAsync_TracksCreneaux_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var horaire = await SeedHoraireAsync(context);
        var sut = new CreneauRepository(context);
        Creneau[] creneaux =
        [
            new() { HoraireSiteId = horaire.Id, Ordre = 1, HeureDebut = new TimeOnly(9, 0), HeureFin = new TimeOnly(10, 45) },
            new() { HoraireSiteId = horaire.Id, Ordre = 2, HeureDebut = new TimeOnly(11, 0), HeureFin = new TimeOnly(12, 45) }
        ];

        await sut.AddRangeAsync(creneaux);
        await context.SaveChangesAsync();

        Assert.All(creneaux, c => Assert.True(c.Id > 0));
        Assert.Equal(2, await context.Creneaux.CountAsync());
    }
}
