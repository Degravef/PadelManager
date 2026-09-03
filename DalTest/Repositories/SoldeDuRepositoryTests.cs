using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class SoldeDuRepositoryTests
{
    private static async Task<(Membre membre, Match match)> SeedMembreAndMatchAsync(PadelDbContext context, string matricule = "L1")
    {
        var membre = new Membre { Matricule = matricule, Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.LibreId };
        context.Membres.Add(membre);
        await context.SaveChangesAsync();
        var match = new Match
        {
            TerrainId = 1, Date = new DateOnly(2026, 9, 1), StartTime = new TimeOnly(10, 0),
            TypeMatch = TypeMatch.Public, Statut = StatutMatch.Played, OrganisateurId = membre.Id
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        return (membre, match);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSoldeDu_ReturnsSoldeDu()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (membre, match) = await SeedMembreAndMatchAsync(context);
        var solde = new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 15m, DateCreation = DateTime.UtcNow };
        context.SoldesDus.Add(solde);
        await context.SaveChangesAsync();

        var sut = new SoldeDuRepository(context);
        var result = await sut.GetByIdAsync(solde.Id);

        Assert.NotNull(result);
        Assert.Equal(15m, result.Montant);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SoldeDuRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMembreIdAsync_ReturnsAllSoldesRegardlessOfStatut()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (membre, match) = await SeedMembreAndMatchAsync(context);
        context.SoldesDus.AddRange(
            new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 15m, DateCreation = DateTime.UtcNow, Statut = StatutSoldeDu.Du },
            new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 30m, DateCreation = DateTime.UtcNow, Statut = StatutSoldeDu.Paye });
        await context.SaveChangesAsync();

        var sut = new SoldeDuRepository(context);
        var result = (await sut.GetByMembreIdAsync(membre.Id)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByMembreIdAsync_NoSoldesForMembre_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SoldeDuRepository(context);

        Assert.Empty(await sut.GetByMembreIdAsync(999));
    }

    [Fact]
    public async Task GetOutstandingByMembreIdAsync_ReturnsOnlyDuStatut()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (membre, match) = await SeedMembreAndMatchAsync(context);
        context.SoldesDus.AddRange(
            new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 15m, DateCreation = DateTime.UtcNow, Statut = StatutSoldeDu.Du },
            new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 30m, DateCreation = DateTime.UtcNow, Statut = StatutSoldeDu.Paye });
        await context.SaveChangesAsync();

        var sut = new SoldeDuRepository(context);
        var result = (await sut.GetOutstandingByMembreIdAsync(membre.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(StatutSoldeDu.Du, result[0].Statut);
    }

    [Fact]
    public async Task GetByMatchIdAsync_ReturnsSoldeForThatMatch()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (membre, match) = await SeedMembreAndMatchAsync(context);
        var solde = new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 15m, DateCreation = DateTime.UtcNow };
        context.SoldesDus.Add(solde);
        await context.SaveChangesAsync();

        var sut = new SoldeDuRepository(context);
        var result = await sut.GetByMatchIdAsync(match.Id);

        Assert.NotNull(result);
        Assert.Equal(solde.Id, result.Id);
    }

    [Fact]
    public async Task GetByMatchIdAsync_UnknownMatch_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new SoldeDuRepository(context);

        Assert.Null(await sut.GetByMatchIdAsync(999));
    }

    [Fact]
    public async Task AddAsync_TracksSoldeDu_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var (membre, match) = await SeedMembreAndMatchAsync(context);
        var sut = new SoldeDuRepository(context);
        var solde = new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 15m, DateCreation = DateTime.UtcNow };

        await sut.AddAsync(solde);
        await context.SaveChangesAsync();

        Assert.True(solde.Id > 0);
        Assert.Equal(1, await context.SoldesDus.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedSoldeDu_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int membreId, matchId, soldeId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var (membre, match) = await SeedMembreAndMatchAsync(seedContext);
            membreId = membre.Id;
            matchId = match.Id;
            var solde = new SoldeDu { MembreId = membreId, MatchId = matchId, Montant = 15m, DateCreation = DateTime.UtcNow, Statut = StatutSoldeDu.Du };
            seedContext.SoldesDus.Add(solde);
            await seedContext.SaveChangesAsync();
            soldeId = solde.Id;
        }

        var detached = new SoldeDu
        {
            Id = soldeId, MembreId = membreId, MatchId = matchId, Montant = 15m,
            DateCreation = DateTime.UtcNow, Statut = StatutSoldeDu.Paye, DateReglement = DateTime.UtcNow
        };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new SoldeDuRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.SoldesDus.FirstAsync(s => s.Id == soldeId);
        Assert.Equal(StatutSoldeDu.Paye, reloaded.Statut);
        Assert.NotNull(reloaded.DateReglement);
    }
}
