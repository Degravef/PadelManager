using Core.Constants;
using Core.Domain.Entities;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class PenaliteRepositoryTests
{
    private static async Task<Membre> SeedMembreAsync(PadelDbContext context, string matricule = "L1")
    {
        var membre = new Membre { Matricule = matricule, Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.LibreId };
        context.Membres.Add(membre);
        await context.SaveChangesAsync();
        return membre;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPenalite_ReturnsPenalite()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var penalite = new Penalite { MembreId = membre.Id, Motif = "Joueurs manquants", DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8) };
        context.Penalites.Add(penalite);
        await context.SaveChangesAsync();

        var sut = new PenaliteRepository(context);
        var result = await sut.GetByIdAsync(penalite.Id);

        Assert.NotNull(result);
        Assert.Equal("Joueurs manquants", result.Motif);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PenaliteRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMembreIdAsync_ReturnsAllPenalitesRegardlessOfActive()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        context.Penalites.AddRange(
            new Penalite { MembreId = membre.Id, Motif = "Ancienne", DateDebut = new DateOnly(2026, 1, 1), DateFin = new DateOnly(2026, 1, 8), Active = false },
            new Penalite { MembreId = membre.Id, Motif = "Active", DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = true });
        await context.SaveChangesAsync();

        var sut = new PenaliteRepository(context);
        var result = (await sut.GetByMembreIdAsync(membre.Id)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByMembreIdAsync_NoPenalitesForMembre_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PenaliteRepository(context);

        Assert.Empty(await sut.GetByMembreIdAsync(999));
    }

    [Fact]
    public async Task GetActiveByMembreIdAsync_ReturnsOnlyActivePenalites()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        context.Penalites.AddRange(
            new Penalite { MembreId = membre.Id, Motif = "Ancienne", DateDebut = new DateOnly(2026, 1, 1), DateFin = new DateOnly(2026, 1, 8), Active = false },
            new Penalite { MembreId = membre.Id, Motif = "Active", DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = true });
        await context.SaveChangesAsync();

        var sut = new PenaliteRepository(context);
        var result = (await sut.GetActiveByMembreIdAsync(membre.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("Active", result[0].Motif);
    }

    [Fact]
    public async Task AddAsync_TracksPenalite_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = await SeedMembreAsync(context);
        var sut = new PenaliteRepository(context);
        var penalite = new Penalite { MembreId = membre.Id, Motif = "Joueurs manquants", DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8) };

        await sut.AddAsync(penalite);
        await context.SaveChangesAsync();

        Assert.True(penalite.Id > 0);
        Assert.Equal(1, await context.Penalites.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedPenalite_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int membreId, penaliteId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var membre = await SeedMembreAsync(seedContext);
            membreId = membre.Id;
            var penalite = new Penalite { MembreId = membreId, Motif = "Joueurs manquants", DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = true };
            seedContext.Penalites.Add(penalite);
            await seedContext.SaveChangesAsync();
            penaliteId = penalite.Id;
        }

        var detached = new Penalite { Id = penaliteId, MembreId = membreId, Motif = "Joueurs manquants", DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = false };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new PenaliteRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Penalites.FirstAsync(p => p.Id == penaliteId);
        Assert.False(reloaded.Active);
    }
}
