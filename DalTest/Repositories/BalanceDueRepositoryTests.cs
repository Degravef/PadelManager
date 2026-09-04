using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using MatchType = Core.Domain.Enums.MatchType;

namespace DalTest.Repositories;

public class BalanceDueRepositoryTests
{
    private static async Task<(Member member, Match match)> SeedMemberAndMatchAsync(PadelDbContext context, string matricule = "L1")
    {
        var member = new Member { Matricule = matricule, Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.LibreId };
        context.Members.Add(member);
        await context.SaveChangesAsync();
        var match = new Match
        {
            CourtId = 1, Date = new DateOnly(2026, 9, 1), StartTime = new TimeOnly(10, 0),
            Type = MatchType.Public, Status = MatchStatus.Played, OrganizerId = member.Id
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        return (member, match);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBalanceDue_ReturnsBalanceDue()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (member, match) = await SeedMemberAndMatchAsync(context);
        var balanceDue = new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 15m, CreatedAt = DateTime.UtcNow };
        context.BalancesDue.Add(balanceDue);
        await context.SaveChangesAsync();

        var sut = new BalanceDueRepository(context);
        var result = await sut.GetByIdAsync(balanceDue.Id);

        Assert.NotNull(result);
        Assert.Equal(15m, result.Amount);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new BalanceDueRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMemberIdAsync_ReturnsAllBalancesDueRegardlessOfStatus()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (member, match) = await SeedMemberAndMatchAsync(context);
        context.BalancesDue.AddRange(
            new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 15m, CreatedAt = DateTime.UtcNow, Status = BalanceDueStatus.Due },
            new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 30m, CreatedAt = DateTime.UtcNow, Status = BalanceDueStatus.Paid });
        await context.SaveChangesAsync();

        var sut = new BalanceDueRepository(context);
        var result = (await sut.GetByMemberIdAsync(member.Id)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByMemberIdAsync_NoBalancesForMember_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new BalanceDueRepository(context);

        Assert.Empty(await sut.GetByMemberIdAsync(999));
    }

    [Fact]
    public async Task GetOutstandingByMemberIdAsync_ReturnsOnlyDueStatus()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (member, match) = await SeedMemberAndMatchAsync(context);
        context.BalancesDue.AddRange(
            new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 15m, CreatedAt = DateTime.UtcNow, Status = BalanceDueStatus.Due },
            new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 30m, CreatedAt = DateTime.UtcNow, Status = BalanceDueStatus.Paid });
        await context.SaveChangesAsync();

        var sut = new BalanceDueRepository(context);
        var result = (await sut.GetOutstandingByMemberIdAsync(member.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(BalanceDueStatus.Due, result[0].Status);
    }

    [Fact]
    public async Task GetByMatchIdAsync_ReturnsBalanceDueForThatMatch()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (member, match) = await SeedMemberAndMatchAsync(context);
        var balanceDue = new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 15m, CreatedAt = DateTime.UtcNow };
        context.BalancesDue.Add(balanceDue);
        await context.SaveChangesAsync();

        var sut = new BalanceDueRepository(context);
        var result = await sut.GetByMatchIdAsync(match.Id);

        Assert.NotNull(result);
        Assert.Equal(balanceDue.Id, result.Id);
    }

    [Fact]
    public async Task GetByMatchIdAsync_UnknownMatch_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new BalanceDueRepository(context);

        Assert.Null(await sut.GetByMatchIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksBalanceDue_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (member, match) = await SeedMemberAndMatchAsync(context);
        var sut = new BalanceDueRepository(context);
        var balanceDue = new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 15m, CreatedAt = DateTime.UtcNow };

        await sut.AddAsync(balanceDue);
        await context.SaveChangesAsync();

        Assert.True(balanceDue.Id > 0);
        Assert.Equal(1, await context.BalancesDue.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedBalanceDue_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int memberId, matchId, balanceDueId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var (member, match) = await SeedMemberAndMatchAsync(seedContext);
            memberId = member.Id;
            matchId = match.Id;
            var balanceDue = new BalanceDue { MemberId = memberId, MatchId = matchId, Amount = 15m, CreatedAt = DateTime.UtcNow, Status = BalanceDueStatus.Due };
            seedContext.BalancesDue.Add(balanceDue);
            await seedContext.SaveChangesAsync();
            balanceDueId = balanceDue.Id;
        }

        var detached = new BalanceDue
        {
            Id = balanceDueId, MemberId = memberId, MatchId = matchId, Amount = 15m,
            CreatedAt = DateTime.UtcNow, Status = BalanceDueStatus.Paid, SettlementDate = DateTime.UtcNow
        };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new BalanceDueRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.BalancesDue.FirstAsync(s => s.Id == balanceDueId);
        Assert.Equal(BalanceDueStatus.Paid, reloaded.Status);
        Assert.NotNull(reloaded.SettlementDate);
    }
}
