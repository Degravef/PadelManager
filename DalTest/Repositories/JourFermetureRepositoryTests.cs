using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class JourFermetureRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingJourFermeture_ReturnsJourFermeture()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var jour = new JourFermeture { SiteId = null, DateFermeture = new DateOnly(2026, 12, 25), Motif = "Noel" };
        context.JoursFermeture.Add(jour);
        await context.SaveChangesAsync();

        var sut = new JourFermetureRepository(context);
        var result = await sut.GetByIdAsync(jour.Id);

        Assert.NotNull(result);
        Assert.Equal("Noel", result.Motif);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new JourFermetureRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsOnlySiteScopedClosures()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        context.JoursFermeture.AddRange(
            new JourFermeture { SiteId = site.Id, DateFermeture = new DateOnly(2026, 7, 1), Motif = "Travaux" },
            new JourFermeture { SiteId = null, DateFermeture = new DateOnly(2026, 12, 25), Motif = "Noel" });
        await context.SaveChangesAsync();

        var sut = new JourFermetureRepository(context);
        var result = (await sut.GetBySiteIdAsync(site.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("Travaux", result[0].Motif);
    }

    [Fact]
    public async Task GetBySiteIdAsync_NoClosuresForSite_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new JourFermetureRepository(context);

        Assert.Empty(await sut.GetBySiteIdAsync(999));
    }

    [Fact]
    public async Task GetGlobalAsync_ReturnsOnlyClosuresWithNullSiteId()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        context.JoursFermeture.AddRange(
            new JourFermeture { SiteId = site.Id, DateFermeture = new DateOnly(2026, 7, 1), Motif = "Travaux" },
            new JourFermeture { SiteId = null, DateFermeture = new DateOnly(2026, 12, 25), Motif = "Noel" });
        await context.SaveChangesAsync();

        var sut = new JourFermetureRepository(context);
        var result = (await sut.GetGlobalAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Noel", result[0].Motif);
    }

    [Fact]
    public async Task AddAsync_TracksJourFermeture_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new JourFermetureRepository(context);
        var jour = new JourFermeture { SiteId = null, DateFermeture = new DateOnly(2026, 1, 1), Motif = "Nouvel An" };

        await sut.AddAsync(jour);
        await context.SaveChangesAsync();

        Assert.True(jour.Id > 0);
        Assert.Equal(1, await context.JoursFermeture.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedJourFermeture_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int jourId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var jour = new JourFermeture { SiteId = null, DateFermeture = new DateOnly(2026, 1, 1), Motif = "Old Motif" };
            seedContext.JoursFermeture.Add(jour);
            await seedContext.SaveChangesAsync();
            jourId = jour.Id;
        }

        var detached = new JourFermeture { Id = jourId, SiteId = null, DateFermeture = new DateOnly(2026, 1, 1), Motif = "New Motif" };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new JourFermetureRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.JoursFermeture.FirstAsync(j => j.Id == jourId);
        Assert.Equal("New Motif", reloaded.Motif);
    }

    [Fact]
    public async Task Delete_RemovesJourFermeture_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var jour = new JourFermeture { SiteId = null, DateFermeture = new DateOnly(2026, 1, 1), Motif = "A supprimer" };
        context.JoursFermeture.Add(jour);
        await context.SaveChangesAsync();

        var sut = new JourFermetureRepository(context);
        sut.Delete(jour);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.JoursFermeture.CountAsync());
    }
}
