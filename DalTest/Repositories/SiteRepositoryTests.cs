using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class SiteRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingSite_ReturnsSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var sut = new SiteRepository(context);
        var result = await sut.GetByIdAsync(site.Id);

        Assert.NotNull(result);
        Assert.Equal("Padel Club A", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByAdminIdAsync_ReturnsOnlySitesForThatAdmin()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Sites.AddRange(
            new Site { Name = "Site A1", Address = "Addr 1", AdminId = 1 },
            new Site { Name = "Site A2", Address = "Addr 2", AdminId = 1 },
            new Site { Name = "Site B1", Address = "Addr 3", AdminId = 2 });
        await context.SaveChangesAsync();

        var sut = new SiteRepository(context);
        var result = (await sut.GetByAdminIdAsync(1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(1, s.AdminId));
    }

    [Fact]
    public async Task GetByAdminIdAsync_NoSitesForAdmin_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteRepository(context);

        Assert.Empty(await sut.GetByAdminIdAsync(42));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEverySiteRegardlessOfAdmin()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Sites.AddRange(
            new Site { Name = "Site A1", Address = "Addr 1", AdminId = 1 },
            new Site { Name = "Site B1", Address = "Addr 2", AdminId = 2 });
        await context.SaveChangesAsync();

        var sut = new SiteRepository(context);
        var result = (await sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_NoSites_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteRepository(context);

        Assert.Empty(await sut.GetAllAsync());
    }

    [Fact]
    public async Task GetDistinctAdminIdsAsync_ReturnsEachAdminIdOnce()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Sites.AddRange(
            new Site { Name = "Site A1", Address = "Addr 1", AdminId = 1 },
            new Site { Name = "Site A2", Address = "Addr 2", AdminId = 1 },
            new Site { Name = "Site B1", Address = "Addr 3", AdminId = 2 });
        await context.SaveChangesAsync();

        var sut = new SiteRepository(context);
        var result = (await sut.GetDistinctAdminIdsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public async Task GetDistinctAdminIdsAsync_NoSites_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteRepository(context);

        Assert.Empty(await sut.GetDistinctAdminIdsAsync());
    }

    [Fact]
    public async Task AddAsync_TracksSite_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteRepository(context);
        var site = new Site { Name = "New Site", Address = "Addr", AdminId = 1 };

        await sut.AddAsync(site);
        // Le repository ne commit jamais lui-meme (voir AGENTS.md) : c'est le test qui
        // joue le role du IUnitOfWork ici.
        await context.SaveChangesAsync();

        Assert.True(site.Id > 0);
        Assert.Equal(1, await context.Sites.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedSite_PersistedAfterSaveChanges()
    {
        // Entite volontairement "detachee" (nouveau contexte, comme un vrai scope
        // par-requete ASP.NET Core) : sinon EF traquerait deja la modification tout
        // seul et le test ne prouverait rien sur Update() lui-meme.
        var dbName = Guid.NewGuid().ToString();
        int siteId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var site = new Site { Name = "Old Name", Address = "Old Addr", AdminId = 1 };
            seedContext.Sites.Add(site);
            await seedContext.SaveChangesAsync();
            siteId = site.Id;
        }

        var detached = new Site { Id = siteId, Name = "New Name", Address = "New Addr", AdminId = 1 };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new SiteRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Sites.FirstAsync(s => s.Id == siteId);
        Assert.Equal("New Name", reloaded.Name);
        Assert.Equal("New Addr", reloaded.Address);
    }

    [Fact]
    public async Task Delete_RemovesSite_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "To Delete", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var sut = new SiteRepository(context);
        sut.Delete(site);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.Sites.CountAsync());
    }
}