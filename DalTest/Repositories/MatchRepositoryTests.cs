using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class MatchRepositoryTests
{
    private static Match NewMatch(int terrainId, DateOnly date, TimeOnly startTime, int organisateurId = 1) => new()
    {
        TerrainId = terrainId, Date = date, StartTime = startTime,
        TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = organisateurId
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
        Assert.Equal(1, result.TerrainId);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MatchRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByTerrainAndDateAsync_ReturnsOnlyMatchingTerrainAndDate()
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
        var result = (await sut.GetByTerrainAndDateAsync(1, date)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(1, m.TerrainId));
        Assert.All(result, m => Assert.Equal(date, m.Date));
    }

    [Fact]
    public async Task GetByOrganisateurIdAsync_ReturnsOnlyMatchesForThatOrganizer()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var date = new DateOnly(2026, 9, 1);
        context.Matches.AddRange(
            NewMatch(1, date, new TimeOnly(9, 0), organisateurId: 10),
            NewMatch(2, date, new TimeOnly(11, 0), organisateurId: 10),
            NewMatch(1, date, new TimeOnly(13, 0), organisateurId: 20));
        await context.SaveChangesAsync();

        var sut = new MatchRepository(context);
        var result = (await sut.GetByOrganisateurIdAsync(10)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(10, m.OrganisateurId));
    }

    [Fact]
    public async Task GetByOrganisateurIdAsync_NoMatchesForOrganizer_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MatchRepository(context);

        Assert.Empty(await sut.GetByOrganisateurIdAsync(999));
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
}
