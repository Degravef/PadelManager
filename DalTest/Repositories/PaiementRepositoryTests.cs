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
    public async Task GetValidatedBySitesAndPeriodAsync_IncludesPaymentsViaParticipationAndViaSoldeDu()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var terrain = new Terrain { Name = "Court 1", SiteId = site.Id };
        context.Terrains.Add(terrain);
        await context.SaveChangesAsync();
        var match = new Match
        {
            TerrainId = terrain.Id, Date = new DateOnly(2026, 9, 10), StartTime = new TimeOnly(10, 0),
            TypeMatch = TypeMatch.Public, Statut = StatutMatch.Open, OrganisateurId = membre.Id
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        var participation = new Participation { MatchId = match.Id, MembreId = membre.Id, NumeroPlace = 1, MontantDu = 15m };
        var solde = new SoldeDu { MembreId = membre.Id, MatchId = match.Id, Montant = 30m, Statut = StatutSoldeDu.Paye };
        context.Participations.Add(participation);
        context.SoldesDus.Add(solde);
        await context.SaveChangesAsync();
        context.Paiements.AddRange(
            new Paiement { MembreId = membre.Id, ParticipationId = participation.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.Valide },
            new Paiement { MembreId = membre.Id, SoldeDuId = solde.Id, Montant = 30m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.Valide },
            new Paiement { MembreId = membre.Id, ParticipationId = participation.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.EnAttente });
        await context.SaveChangesAsync();

        var sut = new PaiementRepository(context);
        var result = (await sut.GetValidatedBySitesAndPeriodAsync([site.Id], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30))).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(StatutPaiement.Valide, p.Statut));
        Assert.Equal(45m, result.Sum(p => p.Montant));
    }

    [Fact]
    public async Task GetValidatedBySitesAndPeriodAsync_ExcludesOtherSitesAndOutOfPeriodMatches()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var siteA = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr", AdminId = 1 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        var terrainA = new Terrain { Name = "Court A", SiteId = siteA.Id };
        var terrainB = new Terrain { Name = "Court B", SiteId = siteB.Id };
        context.Terrains.AddRange(terrainA, terrainB);
        await context.SaveChangesAsync();
        var matchInPeriodOtherSite = new Match { TerrainId = terrainB.Id, Date = new DateOnly(2026, 9, 10), StartTime = new TimeOnly(10, 0), TypeMatch = TypeMatch.Public, Statut = StatutMatch.Open, OrganisateurId = membre.Id };
        var matchOutOfPeriod = new Match { TerrainId = terrainA.Id, Date = new DateOnly(2026, 10, 10), StartTime = new TimeOnly(10, 0), TypeMatch = TypeMatch.Public, Statut = StatutMatch.Open, OrganisateurId = membre.Id };
        context.Matches.AddRange(matchInPeriodOtherSite, matchOutOfPeriod);
        await context.SaveChangesAsync();
        var pA = new Participation { MatchId = matchInPeriodOtherSite.Id, MembreId = membre.Id, NumeroPlace = 1, MontantDu = 15m };
        var pB = new Participation { MatchId = matchOutOfPeriod.Id, MembreId = membre.Id, NumeroPlace = 1, MontantDu = 15m };
        context.Participations.AddRange(pA, pB);
        await context.SaveChangesAsync();
        context.Paiements.AddRange(
            new Paiement { MembreId = membre.Id, ParticipationId = pA.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.Valide },
            new Paiement { MembreId = membre.Id, ParticipationId = pB.Id, Montant = 15m, DatePaiement = DateTime.UtcNow, MoyenPaiement = "CB", Statut = StatutPaiement.Valide });
        await context.SaveChangesAsync();

        var sut = new PaiementRepository(context);
        var result = await sut.GetValidatedBySitesAndPeriodAsync([siteA.Id], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

        Assert.Empty(result);
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
