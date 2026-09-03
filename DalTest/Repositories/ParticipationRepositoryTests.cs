using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class ParticipationRepositoryTests
{
    private static Match NewMatch(int terrainId = 1) => new()
    {
        TerrainId = terrainId, Date = new DateOnly(2026, 9, 1), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30),
        TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 1
    };

    [Fact]
    public async Task AddAsync_TracksParticipation_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = NewMatch();
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var sut = new ParticipationRepository(context);
        var participation = new Participation { Match = match, MatchId = match.Id, MembreId = 1, NumeroPlace = 1, MontantDu = 15m };

        await sut.AddAsync(participation);
        await context.SaveChangesAsync();

        Assert.True(participation.Id > 0);
        Assert.Equal(1, await context.Participations.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingParticipation_ReturnsParticipationWithMatch()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = NewMatch();
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        var participation = new Participation { MatchId = match.Id, MembreId = 1, NumeroPlace = 1, MontantDu = 15m };
        context.Participations.Add(participation);
        await context.SaveChangesAsync();

        var sut = new ParticipationRepository(context);
        var result = await sut.GetByIdAsync(participation.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.Match);
        Assert.Equal(match.Id, result.Match!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new ParticipationRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMatchIdAsync_ReturnsOnlyParticipationsForThatMatch()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var matchA = NewMatch(1);
        var matchB = NewMatch(2);
        context.Matches.AddRange(matchA, matchB);
        await context.SaveChangesAsync();
        context.Participations.AddRange(
            new Participation { MatchId = matchA.Id, MembreId = 1, NumeroPlace = 1, MontantDu = 15m },
            new Participation { MatchId = matchA.Id, MembreId = 2, NumeroPlace = 2, MontantDu = 15m },
            new Participation { MatchId = matchB.Id, MembreId = 1, NumeroPlace = 1, MontantDu = 15m });
        await context.SaveChangesAsync();

        var sut = new ParticipationRepository(context);
        var result = (await sut.GetByMatchIdAsync(matchA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(matchA.Id, p.MatchId));
    }

    [Fact]
    public async Task GetActiveByMembreIdAsync_ExcludesCancelledParticipations()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = NewMatch();
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        context.Participations.AddRange(
            new Participation { MatchId = match.Id, MembreId = 1, NumeroPlace = 1, Statut = StatutParticipation.Reservee, MontantDu = 15m },
            new Participation { MatchId = match.Id, MembreId = 1, NumeroPlace = 2, Statut = StatutParticipation.Payee, MontantDu = 15m },
            new Participation { MatchId = match.Id, MembreId = 1, NumeroPlace = 3, Statut = StatutParticipation.Annulee, MontantDu = 15m });
        await context.SaveChangesAsync();

        var sut = new ParticipationRepository(context);
        var result = (await sut.GetActiveByMembreIdAsync(1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.NotEqual(StatutParticipation.Annulee, p.Statut));
        Assert.All(result, p => Assert.NotNull(p.Match));
    }

    [Fact]
    public async Task GetActiveByMembreIdAsync_NoParticipations_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new ParticipationRepository(context);

        Assert.Empty(await sut.GetActiveByMembreIdAsync(999));
    }

    [Fact]
    public async Task Update_DetachedParticipation_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int matchId, participationId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var match = NewMatch();
            seedContext.Matches.Add(match);
            await seedContext.SaveChangesAsync();
            matchId = match.Id;
            var participation = new Participation { MatchId = matchId, MembreId = 1, NumeroPlace = 1, Statut = StatutParticipation.Reservee, MontantDu = 15m };
            seedContext.Participations.Add(participation);
            await seedContext.SaveChangesAsync();
            participationId = participation.Id;
        }

        var detached = new Participation { Id = participationId, MatchId = matchId, MembreId = 1, NumeroPlace = 1, Statut = StatutParticipation.Payee, MontantDu = 15m };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new ParticipationRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Participations.FirstAsync(p => p.Id == participationId);
        Assert.Equal(StatutParticipation.Payee, reloaded.Statut);
    }

    [Fact]
    public async Task Delete_ExistingParticipation_RemovedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = NewMatch();
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        var participation = new Participation { MatchId = match.Id, MembreId = 1, NumeroPlace = 1, MontantDu = 15m };
        context.Participations.Add(participation);
        await context.SaveChangesAsync();

        var sut = new ParticipationRepository(context);
        sut.Delete(participation);
        await context.SaveChangesAsync();

        Assert.Equal(0, await context.Participations.CountAsync());
    }
}
