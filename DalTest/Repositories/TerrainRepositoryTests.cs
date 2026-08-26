using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class TerrainRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingTerrain_ReturnsTerrainWithSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var terrain = new Terrain { Name = "Court 1", SiteId = site.Id };
        context.Terrains.Add(terrain);
        await context.SaveChangesAsync();

        var sut = new TerrainRepository(context);
        var result = await sut.GetByIdAsync(terrain.Id);

        Assert.NotNull(result);
        Assert.Equal("Court 1", result.Name);
        Assert.NotNull(result.Site);
        Assert.Equal(1, result.Site!.AdminId);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new TerrainRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByAdminIdAsync_ReturnsOnlyTerrainsOnSitesOwnedByThatAdmin()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 2 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.Terrains.AddRange(
            new Terrain { Name = "Court A1", SiteId = siteA.Id },
            new Terrain { Name = "Court A2", SiteId = siteA.Id },
            new Terrain { Name = "Court B1", SiteId = siteB.Id });
        await context.SaveChangesAsync();

        var sut = new TerrainRepository(context);
        var result = (await sut.GetByAdminIdAsync(1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(siteA.Id, t.SiteId));
    }

    [Fact]
    public async Task GetByAdminIdAsync_WithSiteIdFilter_ReturnsOnlyTerrainsForThatSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 1 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.Terrains.AddRange(
            new Terrain { Name = "Court A1", SiteId = siteA.Id },
            new Terrain { Name = "Court B1", SiteId = siteB.Id });
        await context.SaveChangesAsync();

        var sut = new TerrainRepository(context);
        var result = (await sut.GetByAdminIdAsync(1, siteId: siteB.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("Court B1", result[0].Name);
    }

    [Fact]
    public async Task GetByAdminIdAsync_NoTerrainsForAdmin_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new TerrainRepository(context);

        Assert.Empty(await sut.GetByAdminIdAsync(42));
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsOnlyTerrainsForThatSite_RegardlessOfAdmin()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 2 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.Terrains.AddRange(
            new Terrain { Name = "Court A1", SiteId = siteA.Id },
            new Terrain { Name = "Court A2", SiteId = siteA.Id },
            new Terrain { Name = "Court B1", SiteId = siteB.Id });
        await context.SaveChangesAsync();

        var sut = new TerrainRepository(context);
        var result = (await sut.GetBySiteIdAsync(siteA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(siteA.Id, t.SiteId));
    }

    [Fact]
    public async Task GetBySiteIdAsync_NoTerrainsForSite_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new TerrainRepository(context);

        Assert.Empty(await sut.GetBySiteIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksTerrain_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var sut = new TerrainRepository(context);
        var terrain = new Terrain { Name = "New Court", SiteId = site.Id };

        await sut.AddAsync(terrain);
        await context.SaveChangesAsync();

        Assert.True(terrain.Id > 0);
        Assert.Equal(1, await context.Terrains.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedTerrain_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int siteId, terrainId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
            seedContext.Sites.Add(site);
            await seedContext.SaveChangesAsync();
            siteId = site.Id;

            var terrain = new Terrain { Name = "Old Name", SiteId = siteId };
            seedContext.Terrains.Add(terrain);
            await seedContext.SaveChangesAsync();
            terrainId = terrain.Id;
        }

        var detached = new Terrain { Id = terrainId, Name = "New Name", SiteId = siteId };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new TerrainRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Terrains.FirstAsync(t => t.Id == terrainId);
        Assert.Equal("New Name", reloaded.Name);
    }

    [Fact]
    public async Task Delete_RemovesTerrain_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var terrain = new Terrain { Name = "To Delete", SiteId = site.Id };
        context.Terrains.Add(terrain);
        await context.SaveChangesAsync();

        var sut = new TerrainRepository(context);
        sut.Delete(terrain);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.Terrains.CountAsync());
    }
}
