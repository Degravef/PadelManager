using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Dal;
using Microsoft.EntityFrameworkCore;
using MatchType = Core.Domain.Enums.MatchType;

namespace DalTest;

// Optimistic-concurrency coverage for UnitOfWork.SaveChangesAsync (see IConcurrencyToken):
// two separate DbContext instances (one per InMemory-tracked "request") racing to update the
// same Match row must not silently lose one of the updates.
public class UnitOfWorkTests
{
    private static Match NewMatch() => new()
    {
        CourtId = 1, OrganizerId = 1, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30),
        Type = MatchType.Private, Status = MatchStatus.Open, TotalAmount = 60m
    };

    [Fact]
    public async Task SaveChangesAsync_ModifiedConcurrencyTokenEntity_IncrementsVersion()
    {
        string dbName = Guid.NewGuid().ToString();
        await using var seedContext = TestDbContextFactory.CreateInMemory(dbName);
        Match match = NewMatch();
        seedContext.Matches.Add(match);
        await seedContext.SaveChangesAsync();
        Assert.Equal(0, match.Version);

        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        Match tracked = await context.Matches.FirstAsync(m => m.Id == match.Id);
        tracked.AmountPaid = 15m;
        context.Matches.Update(tracked);
        await new UnitOfWork(context).SaveChangesAsync();

        Assert.Equal(1, tracked.Version);
    }

    [Fact]
    public async Task SaveChangesAsync_TwoWritersRaceTheSameMatch_SecondWriterThrowsConcurrentModificationException()
    {
        string dbName = Guid.NewGuid().ToString();
        await using var seedContext = TestDbContextFactory.CreateInMemory(dbName);
        Match match = NewMatch();
        seedContext.Matches.Add(match);
        await seedContext.SaveChangesAsync();
        int matchId = match.Id;

        // Two independent contexts, each standing in for a separate concurrent request, both
        // load the same row (Version = 0) before either one writes back.
        await using var contextA = TestDbContextFactory.CreateInMemory(dbName);
        Match matchA = await contextA.Matches.AsNoTracking().FirstAsync(m => m.Id == matchId);
        await using var contextB = TestDbContextFactory.CreateInMemory(dbName);
        Match matchB = await contextB.Matches.AsNoTracking().FirstAsync(m => m.Id == matchId);

        matchA.AmountPaid = 15m;
        contextA.Matches.Update(matchA);
        await new UnitOfWork(contextA).SaveChangesAsync(); // first writer: succeeds, Version 0 -> 1

        matchB.AmountPaid = 30m;
        contextB.Matches.Update(matchB);
        var unitOfWorkB = new UnitOfWork(contextB);

        // second writer still holds the stale Version = 0 it read before A committed -> conflict,
        // rolled back and surfaced as a typed, retryable ConflictException rather than a lost update.
        await Assert.ThrowsAsync<ConcurrentModificationException>(() => unitOfWorkB.SaveChangesAsync());

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        Match persisted = await verifyContext.Matches.AsNoTracking().FirstAsync(m => m.Id == matchId);
        Assert.Equal(15m, persisted.AmountPaid); // writer B's update never landed
    }
}
