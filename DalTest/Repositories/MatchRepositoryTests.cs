using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using MatchType = Core.Domain.Enums.MatchType;

namespace DalTest.Repositories;

public class MatchRepositoryTests
{
    private static Match NewMatch(int courtId, DateOnly date, TimeOnly startTime, int organizerId = 1) => new()
    {
        CourtId = courtId, Date = date, StartTime = startTime,
        Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = organizerId
    };

    [Fact]
    public async Task GetByIdAsync_ExistingMatch_ReturnsMatch()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = NewMatch(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var sut = new MatchRepository(context);
        var result = await sut.GetByIdAsync(match.Id);

        Assert.NotNull(result);
        Assert.Equal(1, result.CourtId);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MatchRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByCourtAndDateAsync_ReturnsOnlyMatchingCourtAndDate()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var date = new DateOnly(2026, 9, 1);
        context.Matches.AddRange(
            NewMatch(1, date, new TimeOnly(9, 0)),
            NewMatch(1, date, new TimeOnly(11, 0)),
            NewMatch(1, date.AddDays(1), new TimeOnly(9, 0)),
            NewMatch(2, date, new TimeOnly(9, 0)));
        await context.SaveChangesAsync();

        var sut = new MatchRepository(context);
        var result = (await sut.GetByCourtAndDateAsync(1, date)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(1, m.CourtId));
        Assert.All(result, m => Assert.Equal(date, m.Date));
    }

    [Fact]
    public async Task GetByOrganizerIdAsync_ReturnsOnlyMatchesForThatOrganizer()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var date = new DateOnly(2026, 9, 1);
        context.Matches.AddRange(
            NewMatch(1, date, new TimeOnly(9, 0), organizerId: 10),
            NewMatch(2, date, new TimeOnly(11, 0), organizerId: 10),
            NewMatch(1, date, new TimeOnly(13, 0), organizerId: 20));
        await context.SaveChangesAsync();

        var sut = new MatchRepository(context);
        var result = (await sut.GetByOrganizerIdAsync(10)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(10, m.OrganizerId));
    }

    [Fact]
    public async Task GetByOrganizerIdAsync_NoMatchesForOrganizer_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MatchRepository(context);

        Assert.Empty(await sut.GetByOrganizerIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksMatch_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MatchRepository(context);
        var match = NewMatch(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));

        await sut.AddAsync(match);
        await context.SaveChangesAsync();

        Assert.True(match.Id > 0);
        Assert.Equal(1, await context.Matches.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_IncludesParticipations()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = NewMatch(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        context.Participations.Add(new Participation { MatchId = match.Id, MemberId = 1, SeatNumber = 1, AmountDue = 15m });
        await context.SaveChangesAsync();

        var sut = new MatchRepository(context);
        var result = await sut.GetByIdAsync(match.Id);

        Assert.Single(result!.Participations);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsOnlyMatchesOnThatDate_WithParticipationsIncluded()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var date = new DateOnly(2026, 9, 1);
        var matchOnDate = NewMatch(1, date, new TimeOnly(9, 0));
        var matchOtherDate = NewMatch(1, date.AddDays(1), new TimeOnly(9, 0));
        context.Matches.AddRange(matchOnDate, matchOtherDate);
        await context.SaveChangesAsync();
        context.Participations.Add(new Participation { MatchId = matchOnDate.Id, MemberId = 1, SeatNumber = 1, AmountDue = 15m });
        await context.SaveChangesAsync();

        var sut = new MatchRepository(context);
        var result = (await sut.GetByDateAsync(date)).ToList();

        Assert.Single(result);
        Assert.Equal(matchOnDate.Id, result[0].Id);
        Assert.Single(result[0].Participations);
    }

    [Fact]
    public async Task GetByDateAsync_NoMatchesOnDate_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MatchRepository(context);

        Assert.Empty(await sut.GetByDateAsync(new DateOnly(2026, 9, 1)));
    }

    [Fact]
    public async Task Update_DetachedMatch_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int matchId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var match = NewMatch(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));
            seedContext.Matches.Add(match);
            await seedContext.SaveChangesAsync();
            matchId = match.Id;
        }

        var detached = NewMatch(1, new DateOnly(2026, 9, 1), new TimeOnly(10, 0));
        detached.Id = matchId;
        detached.Status = MatchStatus.Complete;
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new MatchRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Matches.FirstAsync(m => m.Id == matchId);
        Assert.Equal(MatchStatus.Complete, reloaded.Status);
    }
}
