using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class PaiementRepositoryTests
{
    private static async Task<Membre> SeedMembreAsync(PadelDbContext context, string matricule = "L1")
    {
        var membre = new Membre { Matricule = matricule, Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.LibreId };
        context.Membres.Add(membre);
        await context.SaveChangesAsync();
        return membre;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPaiement_ReturnsPaiement()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var paiement = new Paiement { MembreId = membre.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" };
        context.Paiements.Add(paiement);
        await context.SaveChangesAsync();

        var sut = new PaiementRepository(context);
        var result = await sut.GetByIdAsync(paiement.Id);

        Assert.NotNull(result);
        Assert.Equal(15m, result.Montant);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PaiementRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMembreIdAsync_ReturnsOnlyPaiementsForThatMembre()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membreA = await SeedMembreAsync(context, "L1");
        var membreB = await SeedMembreAsync(context, "L2");
        context.Paiements.AddRange(
            new Paiement { MembreId = membreA.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" },
            new Paiement { MembreId = membreA.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" },
            new Paiement { MembreId = membreB.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" });
        await context.SaveChangesAsync();

        var sut = new PaiementRepository(context);
        var result = (await sut.GetByMembreIdAsync(membreA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(membreA.Id, p.MembreId));
    }

    [Fact]
    public async Task GetByMembreIdAsync_NoPaiementsForMembre_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PaiementRepository(context);

        Assert.Empty(await sut.GetByMembreIdAsync(999));
    }

    [Fact]
    public async Task GetByParticipationIdAsync_ReturnsOnlyMatchingParticipation()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var match = new Match
        {
            TerrainId = 1, Date = new DateOnly(2026, 9, 1), StartTime = new TimeOnly(10, 0),
            TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = membre.Id
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        var participation = new Participation { MatchId = match.Id, MembreId = membre.Id, NumeroPlace = 1, MontantDu = 15m };
        context.Participations.Add(participation);
        await context.SaveChangesAsync();
        context.Paiements.AddRange(
            new Paiement { MembreId = membre.Id, ParticipationId = participation.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" },
            new Paiement { MembreId = membre.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" });
        await context.SaveChangesAsync();

        var sut = new PaiementRepository(context);
        var result = (await sut.GetByParticipationIdAsync(participation.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(participation.Id, result[0].ParticipationId);
    }

    [Fact]
    public async Task AddAsync_TracksPaiement_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var sut = new PaiementRepository(context);
        var paiement = new Paiement { MembreId = membre.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB" };

        await sut.AddAsync(paiement);
        await context.SaveChangesAsync();

        Assert.True(paiement.Id > 0);
        Assert.Equal(1, await context.Paiements.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedPaiement_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int membreId, paiementId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var membre = await SeedMembreAsync(seedContext);
            membreId = membre.Id;
            var paiement = new Paiement { MembreId = membreId, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.EnAttente };
            seedContext.Paiements.Add(paiement);
            await seedContext.SaveChangesAsync();
            paiementId = paiement.Id;
        }

        var detached = new Paiement { Id = paiementId, MembreId = membreId, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.Valide };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new PaiementRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Paiements.FirstAsync(p => p.Id == paiementId);
        Assert.Equal(StatutPaiement.Valide, reloaded.Statut);
    }
}
