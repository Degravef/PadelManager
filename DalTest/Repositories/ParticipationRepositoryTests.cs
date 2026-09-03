using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class ParticipationRepositoryTests
{
    [Fact]
    public async Task AddAsync_TracksParticipation_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var match = new Match
        {
            TerrainId = 1, Date = new DateOnly(2026, 9, 1), StartTime = new TimeOnly(10, 0),
            TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 1
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var sut = new ParticipationRepository(context);
        var participation = new Participation { Match = match, MatchId = match.Id, MembreId = 1, NumeroPlace = 1, MontantDu = 15m };

        await sut.AddAsync(participation);
        await context.SaveChangesAsync();

        Assert.True(participation.Id > 0);
        Assert.Equal(1, await context.Participations.CountAsync());
    }
}
