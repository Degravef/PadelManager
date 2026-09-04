using Core.Constants;
using Core.Domain.Entities;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DalTest.Repositories;

public class PenaltyRepositoryTests
{
    private static async Task<Member> SeedMemberAsync(PadelDbContext context, string matricule = "L1")
    {
        var member = new Member { Matricule = matricule, Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.LibreId };
        context.Members.Add(member);
        await context.SaveChangesAsync();
        return member;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPenalty_ReturnsPenalty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var penalty = new Penalty { MemberId = member.Id, Reason = "Joueurs manquants", StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8) };
        context.Penalties.Add(penalty);
        await context.SaveChangesAsync();

        var sut = new PenaltyRepository(context);
        var result = await sut.GetByIdAsync(penalty.Id);

        Assert.NotNull(result);
        Assert.Equal("Joueurs manquants", result.Reason);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PenaltyRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMemberIdAsync_ReturnsAllPenaltiesRegardlessOfActive()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        context.Penalties.AddRange(
            new Penalty { MemberId = member.Id, Reason = "Ancienne", StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 1, 8), Active = false },
            new Penalty { MemberId = member.Id, Reason = "Active", StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = true });
        await context.SaveChangesAsync();

        var sut = new PenaltyRepository(context);
        var result = (await sut.GetByMemberIdAsync(member.Id)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByMemberIdAsync_NoPenaltiesForMember_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PenaltyRepository(context);

        Assert.Empty(await sut.GetByMemberIdAsync(999));
    }

    [Fact]
    public async Task GetActiveByMemberIdAsync_ReturnsOnlyActivePenalties()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        context.Penalties.AddRange(
            new Penalty { MemberId = member.Id, Reason = "Ancienne", StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 1, 8), Active = false },
            new Penalty { MemberId = member.Id, Reason = "Active", StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = true });
        await context.SaveChangesAsync();

        var sut = new PenaltyRepository(context);
        var result = (await sut.GetActiveByMemberIdAsync(member.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("Active", result[0].Reason);
    }

    [Fact]
    public async Task AddAsync_TracksPenalty_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var sut = new PenaltyRepository(context);
        var penalty = new Penalty { MemberId = member.Id, Reason = "Joueurs manquants", StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8) };

        await sut.AddAsync(penalty);
        await context.SaveChangesAsync();

        Assert.True(penalty.Id > 0);
        Assert.Equal(1, await context.Penalties.CountAsync());
    }

    [Fact]
    public async Task Update_DetachedPenalty_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int memberId, penaltyId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var member = await SeedMemberAsync(seedContext);
            memberId = member.Id;
            var penalty = new Penalty { MemberId = memberId, Reason = "Joueurs manquants", StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = true };
            seedContext.Penalties.Add(penalty);
            await seedContext.SaveChangesAsync();
            penaltyId = penalty.Id;
        }

        var detached = new Penalty { Id = penaltyId, MemberId = memberId, Reason = "Joueurs manquants", StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 9, 8), Active = false };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new PenaltyRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Penalties.FirstAsync(p => p.Id == penaltyId);
        Assert.False(reloaded.Active);
    }
}
