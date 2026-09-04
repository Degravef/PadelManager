using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class SiteScheduleRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingSiteSchedule_ReturnsSiteSchedule()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var siteSchedule = new SiteSchedule { SiteId = site.Id, Year = 2026 };
        context.SiteSchedules.Add(siteSchedule);
        await context.SaveChangesAsync();

        var sut = new SiteScheduleRepository(context);
        var result = await sut.GetByIdAsync(siteSchedule.Id);

        Assert.NotNull(result);
        Assert.Equal(2026, result.Year);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteScheduleRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_ExistingCombination_ReturnsSiteSchedule()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        context.SiteSchedules.AddRange(
            new SiteSchedule { SiteId = site.Id, Year = 2025 },
            new SiteSchedule { SiteId = site.Id, Year = 2026 });
        await context.SaveChangesAsync();

        var sut = new SiteScheduleRepository(context);
        var result = await sut.GetBySiteAndYearAsync(site.Id, 2026);

        Assert.NotNull(result);
        Assert.Equal(2026, result.Year);
    }

    [Fact]
    public async Task GetBySiteAndYearAsync_UnknownCombination_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteScheduleRepository(context);

        Assert.Null(await sut.GetBySiteAndYearAsync(999, 2026));
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsOnlySiteSchedulesForThatSite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteA = new Site { Name = "Site A", Address = "Addr 1", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr 2", AdminId = 1 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        context.SiteSchedules.AddRange(
            new SiteSchedule { SiteId = siteA.Id, Year = 2025 },
            new SiteSchedule { SiteId = siteA.Id, Year = 2026 },
            new SiteSchedule { SiteId = siteB.Id, Year = 2026 });
        await context.SaveChangesAsync();

        var sut = new SiteScheduleRepository(context);
        var result = (await sut.GetBySiteIdAsync(siteA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(siteA.Id, h.SiteId));
    }

    [Fact]
    public async Task GetBySiteIdAsync_NoSiteSchedulesForSite_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SiteScheduleRepository(context);

        Assert.Empty(await sut.GetBySiteIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksSiteSchedule_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var sut = new SiteScheduleRepository(context);
        var siteSchedule = new SiteSchedule { SiteId = site.Id, Year = 2026 };

        await sut.AddAsync(siteSchedule);
        await context.SaveChangesAsync();

        Assert.True(siteSchedule.Id > 0);
        Assert.Equal(1, await context.SiteSchedules.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedSiteSchedule_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int siteId, siteScheduleId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
            seedContext.Sites.Add(site);
            await seedContext.SaveChangesAsync();
            siteId = site.Id;

            var siteSchedule = new SiteSchedule { SiteId = siteId, Year = 2026, MatchPrice = 60m };
            seedContext.SiteSchedules.Add(siteSchedule);
            await seedContext.SaveChangesAsync();
            siteScheduleId = siteSchedule.Id;
        }

        var detached = new SiteSchedule { Id = siteScheduleId, SiteId = siteId, Year = 2026, MatchPrice = 70m };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new SiteScheduleRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.SiteSchedules.FirstAsync(h => h.Id == siteScheduleId);
        Assert.Equal(70m, reloaded.MatchPrice);
    }

    [Fact]
    public async Task Delete_RemovesSiteSchedule_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var siteSchedule = new SiteSchedule { SiteId = site.Id, Year = 2026 };
        context.SiteSchedules.Add(siteSchedule);
        await context.SaveChangesAsync();

        var sut = new SiteScheduleRepository(context);
        sut.Delete(siteSchedule);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.SiteSchedules.CountAsync());
    }
}
