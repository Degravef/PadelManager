using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class ClosureDayRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingClosureDay_ReturnsClosureDay()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var closureDay = new ClosureDay { SiteId = null, ClosureDate = new DateOnly(2026, 12, 25), Reason = "Noel" };
        context.ClosureDays.Add(closureDay);
        await context.SaveChangesAsync();

        var sut = new ClosureDayRepository(context);
        var result = await sut.GetByIdAsync(closureDay.Id);

        Assert.NotNull(result);
        Assert.Equal("Noel", result.Reason);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new ClosureDayRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsOnlySiteScopedClosures()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        context.ClosureDays.AddRange(
            new ClosureDay { SiteId = site.Id, ClosureDate = new DateOnly(2026, 7, 1), Reason = "Travaux" },
            new ClosureDay { SiteId = null, ClosureDate = new DateOnly(2026, 12, 25), Reason = "Noel" });
        await context.SaveChangesAsync();

        var sut = new ClosureDayRepository(context);
        var result = (await sut.GetBySiteIdAsync(site.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("Travaux", result[0].Reason);
    }

    [Fact]
    public async Task GetBySiteIdAsync_NoClosuresForSite_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new ClosureDayRepository(context);

        Assert.Empty(await sut.GetBySiteIdAsync(999));
    }

    [Fact]
    public async Task GetGlobalAsync_ReturnsOnlyClosuresWithNullSiteId()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        context.ClosureDays.AddRange(
            new ClosureDay { SiteId = site.Id, ClosureDate = new DateOnly(2026, 7, 1), Reason = "Travaux" },
            new ClosureDay { SiteId = null, ClosureDate = new DateOnly(2026, 12, 25), Reason = "Noel" });
        await context.SaveChangesAsync();

        var sut = new ClosureDayRepository(context);
        var result = (await sut.GetGlobalAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Noel", result[0].Reason);
    }

    [Fact]
    public async Task AddAsync_TracksClosureDay_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new ClosureDayRepository(context);
        var closureDay = new ClosureDay { SiteId = null, ClosureDate = new DateOnly(2026, 1, 1), Reason = "Nouvel An" };

        await sut.AddAsync(closureDay);
        await context.SaveChangesAsync();

        Assert.True(closureDay.Id > 0);
        Assert.Equal(1, await context.ClosureDays.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedClosureDay_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int closureDayId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var closureDay = new ClosureDay { SiteId = null, ClosureDate = new DateOnly(2026, 1, 1), Reason = "Old Reason" };
            seedContext.ClosureDays.Add(closureDay);
            await seedContext.SaveChangesAsync();
            closureDayId = closureDay.Id;
        }

        var detached = new ClosureDay { Id = closureDayId, SiteId = null, ClosureDate = new DateOnly(2026, 1, 1), Reason = "New Reason" };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new ClosureDayRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.ClosureDays.FirstAsync(j => j.Id == closureDayId);
        Assert.Equal("New Reason", reloaded.Reason);
    }

    [Fact]
    public async Task Delete_RemovesClosureDay_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var closureDay = new ClosureDay { SiteId = null, ClosureDate = new DateOnly(2026, 1, 1), Reason = "A supprimer" };
        context.ClosureDays.Add(closureDay);
        await context.SaveChangesAsync();

        var sut = new ClosureDayRepository(context);
        sut.Delete(closureDay);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.ClosureDays.CountAsync());
    }
}
