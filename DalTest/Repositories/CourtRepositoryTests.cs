using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class CourtRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingCourt_ReturnsCourtWithSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var court = new Court { Name = "Court 1", SiteId = site.Id };
        context.Courts.Add(court);
        await context.SaveChangesAsync();

        var sut = new CourtRepository(context);
        var result = await sut.GetByIdAsync(court.Id);

        Assert.NotNull(result);
        Assert.Equal("Court 1", result.Name);
        Assert.NotNull(result.Site);
        Assert.Equal(1, result.Site!.AdminId);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new CourtRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByAdminIdAsync_ReturnsOnlyCourtsOnSitesOwnedByThatAdmin()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 2 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.Courts.AddRange(
            new Court { Name = "Court A1", SiteId = siteA.Id },
            new Court { Name = "Court A2", SiteId = siteA.Id },
            new Court { Name = "Court B1", SiteId = siteB.Id });
        await context.SaveChangesAsync();

        var sut = new CourtRepository(context);
        var result = (await sut.GetByAdminIdAsync(1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(siteA.Id, t.SiteId));
    }

    [Fact]
    public async Task GetByAdminIdAsync_WithSiteIdFilter_ReturnsOnlyCourtsForThatSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 1 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.Courts.AddRange(
            new Court { Name = "Court A1", SiteId = siteA.Id },
            new Court { Name = "Court B1", SiteId = siteB.Id });
        await context.SaveChangesAsync();

        var sut = new CourtRepository(context);
        var result = (await sut.GetByAdminIdAsync(1, siteId: siteB.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("Court B1", result[0].Name);
    }

    [Fact]
    public async Task GetByAdminIdAsync_NoCourtsForAdmin_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new CourtRepository(context);

        Assert.Empty(await sut.GetByAdminIdAsync(42));
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsOnlyCourtsForThatSite_RegardlessOfAdmin()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 2 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.Courts.AddRange(
            new Court { Name = "Court A1", SiteId = siteA.Id },
            new Court { Name = "Court A2", SiteId = siteA.Id },
            new Court { Name = "Court B1", SiteId = siteB.Id });
        await context.SaveChangesAsync();

        var sut = new CourtRepository(context);
        var result = (await sut.GetBySiteIdAsync(siteA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(siteA.Id, t.SiteId));
    }

    [Fact]
    public async Task GetBySiteIdAsync_NoCourtsForSite_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new CourtRepository(context);

        Assert.Empty(await sut.GetBySiteIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksCourt_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var sut = new CourtRepository(context);
        var court = new Court { Name = "New Court", SiteId = site.Id };

        await sut.AddAsync(court);
        await context.SaveChangesAsync();

        Assert.True(court.Id > 0);
        Assert.Equal(1, await context.Courts.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedCourt_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int siteId, courtId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
            seedContext.Sites.Add(site);
            await seedContext.SaveChangesAsync();
            siteId = site.Id;

            var court = new Court { Name = "Old Name", SiteId = siteId };
            seedContext.Courts.Add(court);
            await seedContext.SaveChangesAsync();
            courtId = court.Id;
        }

        var detached = new Court { Id = courtId, Name = "New Name", SiteId = siteId };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new CourtRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Courts.FirstAsync(t => t.Id == courtId);
        Assert.Equal("New Name", reloaded.Name);
    }

    [Fact]
    public async Task Delete_RemovesCourt_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var court = new Court { Name = "To Delete", SiteId = site.Id };
        context.Courts.Add(court);
        await context.SaveChangesAsync();

        var sut = new CourtRepository(context);
        sut.Delete(court);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.Courts.CountAsync());
    }
}
