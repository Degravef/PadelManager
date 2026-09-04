using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Dal;
using Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using MatchType = Core.Domain.Enums.MatchType;

namespace DalTest.Repositories;

public class PaymentRepositoryTests
{
    private static async Task<Member> SeedMemberAsync(PadelDbContext context, string matricule = "L1")
    {
        var member = new Member { Matricule = matricule, Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.LibreId };
        context.Members.Add(member);
        await context.SaveChangesAsync();
        return member;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPayment_ReturnsPayment()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var payment = new Payment { MemberId = member.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" };
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);
        var result = await sut.GetByIdAsync(payment.Id);

        Assert.NotNull(result);
        Assert.Equal(15m, result.Amount);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PaymentRepository(context);

        Assert.Null(await sut.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByMemberIdAsync_ReturnsOnlyPaymentsForThatMember()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var memberA = await SeedMemberAsync(context, "L1");
        var memberB = await SeedMemberAsync(context, "L2");
        context.Payments.AddRange(
            new Payment { MemberId = memberA.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" },
            new Payment { MemberId = memberA.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" },
            new Payment { MemberId = memberB.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" });
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);
        var result = (await sut.GetByMemberIdAsync(memberA.Id)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(memberA.Id, p.MemberId));
    }

    [Fact]
    public async Task GetByMemberIdAsync_NoPaymentsForMember_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var sut = new PaymentRepository(context);

        Assert.Empty(await sut.GetByMemberIdAsync(999));
    }

    [Fact]
    public async Task GetByParticipationIdAsync_ReturnsOnlyMatchingParticipation()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var match = new Match
        {
            CourtId = 1, Date = new DateOnly(2026, 9, 1), StartTime = new TimeOnly(10, 0),
            Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = member.Id
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        var participation = new Participation { MatchId = match.Id, MemberId = member.Id, SeatNumber = 1, AmountDue = 15m };
        context.Participations.Add(participation);
        await context.SaveChangesAsync();
        context.Payments.AddRange(
            new Payment { MemberId = member.Id, ParticipationId = participation.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" },
            new Payment { MemberId = member.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" });
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);
        var result = (await sut.GetByParticipationIdAsync(participation.Id)).ToList();

        Assert.Single(result);
        Assert.Equal(participation.Id, result[0].ParticipationId);
    }

    [Fact]
    public async Task AddAsync_TracksPayment_PersistedAfterSaveChanges()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var sut = new PaymentRepository(context);
        var payment = new Payment { MemberId = member.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB" };

        await sut.AddAsync(payment);
        await context.SaveChangesAsync();

        Assert.True(payment.Id > 0);
        Assert.Equal(1, await context.Payments.CountAsync());
    }

    [Fact]
    public async Task GetValidatedBySitesAndPeriodAsync_IncludesPaymentsViaParticipationAndViaBalanceDue()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var site = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        var court = new Court { Name = "Court 1", SiteId = site.Id };
        context.Courts.Add(court);
        await context.SaveChangesAsync();
        var match = new Match
        {
            CourtId = court.Id, Date = new DateOnly(2026, 9, 10), StartTime = new TimeOnly(10, 0),
            Type = MatchType.Public, Status = MatchStatus.Open, OrganizerId = member.Id
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        var participation = new Participation { MatchId = match.Id, MemberId = member.Id, SeatNumber = 1, AmountDue = 15m };
        var balanceDue = new BalanceDue { MemberId = member.Id, MatchId = match.Id, Amount = 30m, Status = BalanceDueStatus.Paid };
        context.Participations.Add(participation);
        context.BalancesDue.Add(balanceDue);
        await context.SaveChangesAsync();
        context.Payments.AddRange(
            new Payment { MemberId = member.Id, ParticipationId = participation.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Validated },
            new Payment { MemberId = member.Id, BalanceDueId = balanceDue.Id, Amount = 30m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Validated },
            new Payment { MemberId = member.Id, ParticipationId = participation.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Pending });
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);
        var result = (await sut.GetValidatedBySitesAndPeriodAsync([site.Id], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30))).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(PaymentStatus.Validated, p.Status));
        Assert.Equal(45m, result.Sum(p => p.Amount));
    }

    [Fact]
    public async Task GetValidatedBySitesAndPeriodAsync_ExcludesOtherSitesAndOutOfPeriodMatches()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var member = await SeedMemberAsync(context);
        var siteA = new Site { Name = "Site A", Address = "Addr", AdminId = 1 };
        var siteB = new Site { Name = "Site B", Address = "Addr", AdminId = 1 };
        context.Sites.AddRange(siteA, siteB);
        await context.SaveChangesAsync();
        var courtA = new Court { Name = "Court A", SiteId = siteA.Id };
        var courtB = new Court { Name = "Court B", SiteId = siteB.Id };
        context.Courts.AddRange(courtA, courtB);
        await context.SaveChangesAsync();
        var matchInPeriodOtherSite = new Match { CourtId = courtB.Id, Date = new DateOnly(2026, 9, 10), StartTime = new TimeOnly(10, 0), Type = MatchType.Public, Status = MatchStatus.Open, OrganizerId = member.Id };
        var matchOutOfPeriod = new Match { CourtId = courtA.Id, Date = new DateOnly(2026, 10, 10), StartTime = new TimeOnly(10, 0), Type = MatchType.Public, Status = MatchStatus.Open, OrganizerId = member.Id };
        context.Matches.AddRange(matchInPeriodOtherSite, matchOutOfPeriod);
        await context.SaveChangesAsync();
        var pA = new Participation { MatchId = matchInPeriodOtherSite.Id, MemberId = member.Id, SeatNumber = 1, AmountDue = 15m };
        var pB = new Participation { MatchId = matchOutOfPeriod.Id, MemberId = member.Id, SeatNumber = 1, AmountDue = 15m };
        context.Participations.AddRange(pA, pB);
        await context.SaveChangesAsync();
        context.Payments.AddRange(
            new Payment { MemberId = member.Id, ParticipationId = pA.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Validated },
            new Payment { MemberId = member.Id, ParticipationId = pB.Id, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Validated });
        await context.SaveChangesAsync();

        var sut = new PaymentRepository(context);
        var result = await sut.GetValidatedBySitesAndPeriodAsync([siteA.Id], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

        Assert.Empty(result);
    }

    [Fact]
    public async Task Update_DetachedPayment_PersistedAfterSaveChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int memberId, paymentId;
        await using (var seedContext = TestDbContextFactory.CreateInMemory(dbName))
        {
            var member = await SeedMemberAsync(seedContext);
            memberId = member.Id;
            var payment = new Payment { MemberId = memberId, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Pending };
            seedContext.Payments.Add(payment);
            await seedContext.SaveChangesAsync();
            paymentId = payment.Id;
        }

        var detached = new Payment { Id = paymentId, MemberId = memberId, Amount = 15m, PaymentDate = DateTime.UtcNow, PaymentMethod = "CB", Status = PaymentStatus.Validated };
        await using var context = TestDbContextFactory.CreateInMemory(dbName);
        var sut = new PaymentRepository(context);
        sut.Update(detached);
        await context.SaveChangesAsync();

        await using var verifyContext = TestDbContextFactory.CreateInMemory(dbName);
        var reloaded = await verifyContext.Payments.FirstAsync(p => p.Id == paymentId);
        Assert.Equal(PaymentStatus.Validated, reloaded.Status);
    }
}
