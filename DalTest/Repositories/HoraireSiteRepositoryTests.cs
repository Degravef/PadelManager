using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class HoraireSiteRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingHoraireSite_ReturnsHoraireSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var horaire = new HoraireSite { SiteId = site.Id, Annee = 2026 };
        context.HorairesSites.Add(horaire);
        await context.SaveChangesAsync();

        var sut = new HoraireSiteRepository(context);
        var result = await sut.GetByIdAsync(horaire.Id);

        Assert.NotNull(result);
        Assert.Equal(2026, result.Annee);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new HoraireSiteRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_ExistingCombination_ReturnsHoraireSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        context.HorairesSites.AddRange(
            new HoraireSite { SiteId = site.Id, Annee = 2025 },
            new HoraireSite { SiteId = site.Id, Annee = 2026 });
        await context.SaveChangesAsync();

        var sut = new HoraireSiteRepository(context);
        var result = await sut.GetBySiteAndYearAsync(site.Id, 2026);

        Assert.NotNull(result);
        Assert.Equal(2026, result.Annee);
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_UnknownCombination_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new HoraireSiteRepository(context);

        Assert.Null(await sut.GetBySiteAndYearAsync(999, 2026));
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsOnlyHorairesForThatSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 1 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.HorairesSites.AddRange(
            new HoraireSite { SiteId = siteA.Id, Annee = 2025 },
            new HoraireSite { SiteId = siteA.Id, Annee = 2026 },
            new HoraireSite { SiteId = siteB.Id, Annee = 2026 });
        await context.SaveChangesAsync();

        var sut = new HoraireSiteRepository(context);
        var result = (await sut.GetBySiteIdAsync(siteA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(siteA.Id, h.SiteId));
    }

    [Fact]
    public async Task GetBySiteIdAsync_NoHorairesForSite_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new HoraireSiteRepository(context);

        Assert.Empty(await sut.GetBySiteIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksHoraireSite_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var sut = new HoraireSiteRepository(context);
        var horaire = new HoraireSite { SiteId = site.Id, Annee = 2026 };

        await sut.AddAsync(horaire);
        await context.SaveChangesAsync();

        Assert.True(horaire.Id > 0);
        Assert.Equal(1, await context.HorairesSites.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedHoraireSite_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int siteId, horaireId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
            seedContext.Sites.Add(site);
            await seedContext.SaveChangesAsync();
            siteId = site.Id;

            var horaire = new HoraireSite { SiteId = siteId, Annee = 2026, PrixMatch = 60m };
            seedContext.HorairesSites.Add(horaire);
            await seedContext.SaveChangesAsync();
            horaireId = horaire.Id;
        }

        var detached = new HoraireSite { Id = horaireId, SiteId = siteId, Annee = 2026, PrixMatch = 70m };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new HoraireSiteRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.HorairesSites.FirstAsync(h => h.Id == horaireId);
        Assert.Equal(70m, reloaded.PrixMatch);
    }

    [Fact]
    public async Task Delete_RemovesHoraireSite_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var horaire = new HoraireSite { SiteId = site.Id, Annee = 2026 };
        context.HorairesSites.Add(horaire);
        await context.SaveChangesAsync();

        var sut = new HoraireSiteRepository(context);
        sut.Delete(horaire);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.HorairesSites.CountAsync());
    }
}
