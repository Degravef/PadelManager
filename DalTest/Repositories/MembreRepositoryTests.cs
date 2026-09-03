using Core.Constants;
using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class MembreRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingMembre_ReturnsMembre()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId };
        context.Membres.Add(membre);
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);
        var result = await sut.GetByIdAsync(membre.Id);

        Assert.NotNull(result);
        Assert.Equal("Doe", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MembreRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMatriculeAsync_ExistingMembre_ReturnsMembre()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId };
        context.Membres.Add(membre);
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);
        var result = await sut.GetByMatriculeAsync("G1");

        Assert.NotNull(result);
        Assert.Equal("Doe", result.Name);
    }

    [Fact]
    public async Task GetByMatriculeAsync_UnknownMatricule_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MembreRepository(context);

        Assert.Null(await sut.GetByMatriculeAsync("G999"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryMembre()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Membres.AddRange(
            new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId },
            new Membre { Matricule = "L1", Name = "Roe", FirstName = "Jim", TypeMembreId = TypeMembreSeed.LibreId });
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);
        var result = (await sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllMatriculesAsync_ReturnsEveryMatricule()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Membres.AddRange(
            new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId },
            new Membre { Matricule = "L1", Name = "Roe", FirstName = "Jim", TypeMembreId = TypeMembreSeed.LibreId });
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);
        var result = (await sut.GetAllMatriculesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains("G1", result);
        Assert.Contains("L1", result);
    }

    [Fact]
    public async Task GetAllMatriculesAsync_NoMembres_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MembreRepository(context);

        Assert.Empty(await sut.GetAllMatriculesAsync());
    }

    [Fact]
    public async Task GetMatriculesByPrefixAsync_ReturnsOnlyMatchingPrefix()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Membres.AddRange(
            new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId },
            new Membre { Matricule = "G2", Name = "Roe", FirstName = "Jim", TypeMembreId = TypeMembreSeed.GlobalId },
            new Membre { Matricule = "L1", Name = "Poe", FirstName = "Al", TypeMembreId = TypeMembreSeed.LibreId });
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);
        var result = (await sut.GetMatriculesByPrefixAsync("G")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains("G1", result);
        Assert.Contains("G2", result);
    }

    [Fact]
    public async Task GetMatriculesByPrefixAsync_NoMatch_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Membres.Add(new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId });
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);

        Assert.Empty(await sut.GetMatriculesByPrefixAsync("S"));
    }

    [Fact]
    public async Task GetByMatriculeAsync_IncludesSoldesDusAndPenalites()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var membre = new Membre { Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId };
        context.Membres.Add(membre);
        await context.SaveChangesAsync();
        context.SoldesDus.Add(new SoldeDu { MembreId = membre.Id, MatchId = 1, Montant = 15m });
        context.Penalites.Add(new Penalite { MembreId = membre.Id, DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = true });
        await context.SaveChangesAsync();

        var sut = new MembreRepository(context);
        var result = await sut.GetByMatriculeAsync("G1");

        Assert.NotNull(result);
        Assert.Single(result.SoldesDus);
        Assert.Single(result.Penalites);
    }

    [Fact]
    public async Task AddAsync_TracksMembre_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MembreRepository(context);
        var membre = new Membre { Matricule = "L1", Name = "Doe", FirstName = "John", TypeMembreId = TypeMembreSeed.LibreId };

        await sut.AddAsync(membre);
        
        
        await context.SaveChangesAsync();

        Assert.True(membre.Id > 0);
        Assert.Equal(1, await context.Membres.CountAsync());
    }
}
