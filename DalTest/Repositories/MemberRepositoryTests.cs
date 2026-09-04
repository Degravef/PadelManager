using Core.Constants;
using Core.Domain.Entities;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class MemberRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingMember_ReturnsMember()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);
        var result = await sut.GetByIdAsync(member.Id);

        Assert.NotNull(result);
        Assert.Equal("Doe", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MemberRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMatriculeAsync_ExistingMember_ReturnsMember()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);
        var result = await sut.GetByMatriculeAsync("G1");

        Assert.NotNull(result);
        Assert.Equal("Doe", result.Name);
    }

    [Fact]
    public async Task GetByMatriculeAsync_UnknownMatricule_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MemberRepository(context);

        Assert.Null(await sut.GetByMatriculeAsync("G999"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryMember()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Members.AddRange(
            new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId },
            new Member { Matricule = "L1", Name = "Roe", FirstName = "Jim", MemberTypeId = MemberTypeSeed.LibreId });
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);
        var result = (await sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllMatriculesAsync_ReturnsEveryMatricule()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Members.AddRange(
            new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId },
            new Member { Matricule = "L1", Name = "Roe", FirstName = "Jim", MemberTypeId = MemberTypeSeed.LibreId });
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);
        var result = (await sut.GetAllMatriculesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains("G1", result);
        Assert.Contains("L1", result);
    }

    [Fact]
    public async Task GetAllMatriculesAsync_NoMembers_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MemberRepository(context);

        Assert.Empty(await sut.GetAllMatriculesAsync());
    }

    [Fact]
    public async Task GetMatriculesByPrefixAsync_ReturnsOnlyMatchingPrefix()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Members.AddRange(
            new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId },
            new Member { Matricule = "G2", Name = "Roe", FirstName = "Jim", MemberTypeId = MemberTypeSeed.GlobalId },
            new Member { Matricule = "L1", Name = "Poe", FirstName = "Al", MemberTypeId = MemberTypeSeed.LibreId });
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);
        var result = (await sut.GetMatriculesByPrefixAsync("G")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains("G1", result);
        Assert.Contains("G2", result);
    }

    [Fact]
    public async Task GetMatriculesByPrefixAsync_NoMatch_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Members.Add(new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId });
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);

        Assert.Empty(await sut.GetMatriculesByPrefixAsync("S"));
    }

    [Fact]
    public async Task GetByMatriculeAsync_IncludesBalancesDueAndPenalties()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = new Member { Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId };
        context.Members.Add(member);
        await context.SaveChangesAsync();
        context.BalancesDue.Add(new BalanceDue { MemberId = member.Id, MatchId = 1, Amount = 15m });
        context.Penalties.Add(new Penalty { MemberId = member.Id, StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = true });
        await context.SaveChangesAsync();

        var sut = new MemberRepository(context);
        var result = await sut.GetByMatriculeAsync("G1");

        Assert.NotNull(result);
        Assert.Single(result.BalancesDue);
        Assert.Single(result.Penalties);
    }

    [Fact]
    public async Task AddAsync_TracksMember_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new MemberRepository(context);
        var member = new Member { Matricule = "L1", Name = "Doe", FirstName = "John", MemberTypeId = MemberTypeSeed.LibreId };

        await sut.AddAsync(member);
        // The repository never commits itself (see AGENTS.md): the test plays the
        // role of the IUnitOfWork here.
        await context.SaveChangesAsync();

        Assert.True(member.Id > 0);
        Assert.Equal(1, await context.Members.CountAsync());
    }
}
