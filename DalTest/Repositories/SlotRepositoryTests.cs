using Core.Domain.Entities;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class SlotRepositoryTests
{
    private static async Task<SiteSchedule> SeedSiteScheduleAsync(PadelDbContext context, int siteAdminId = 1)
    {
        var site = new Site { Name = "Padel Club A", Address = "Rue A 1", AdminId = siteAdminId };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var siteSchedule = new SiteSchedule { SiteId = site.Id, Year = 2026 };
        context.SiteSchedules.Add(siteSchedule);
        await context.SaveChangesAsync();
        return siteSchedule;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSlot_ReturnsSlot()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteSchedule = await SeedSiteScheduleAsync(context);
        var slot = new Slot { SiteScheduleId = siteSchedule.Id, Order = 1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 45) };
        context.Slots.Add(slot);
        await context.SaveChangesAsync();

        var sut = new SlotRepository(context);
        var result = await sut.GetByIdAsync(slot.Id);

        Assert.NotNull(result);
        Assert.Equal(1, result.Order);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SlotRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetBySiteScheduleIdAsync_ReturnsOnlyMatchingSiteSchedule_OrderedByOrder()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteScheduleA = await SeedSiteScheduleAsync(context);
        var siteScheduleB = await SeedSiteScheduleAsync(context, siteAdminId: 2);
        context.Slots.AddRange(
            new Slot { SiteScheduleId = siteScheduleA.Id, Order = 2, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 45) },
            new Slot { SiteScheduleId = siteScheduleA.Id, Order = 1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 45) },
            new Slot { SiteScheduleId = siteScheduleB.Id, Order = 1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 45) });
        await context.SaveChangesAsync();

        var sut = new SlotRepository(context);
        var result = (await sut.GetBySiteScheduleIdAsync(siteScheduleA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(siteScheduleA.Id, c.SiteScheduleId));
        Assert.Equal([1, 2], result.Select(c => c.Order));
    }

    [Fact]
    public async Task GetBySiteScheduleIdAsync_NoSlotsForSiteSchedule_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SlotRepository(context);

        Assert.Empty(await sut.GetBySiteScheduleIdAsync(999));
    }

    [Fact]
    public async Task AddRangeAsync_TracksSlots_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var siteSchedule = await SeedSiteScheduleAsync(context);
        var sut = new SlotRepository(context);
        Slot[] slots =
        [
            new() { SiteScheduleId = siteSchedule.Id, Order = 1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 45) },
            new() { SiteScheduleId = siteSchedule.Id, Order = 2, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 45) }
        ];

        await sut.AddRangeAsync(slots);
        await context.SaveChangesAsync();

        Assert.All(slots, c => Assert.True(c.Id > 0));
        Assert.Equal(2, await context.Slots.CountAsync());
    }
}
