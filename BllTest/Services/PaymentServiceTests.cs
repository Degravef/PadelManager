using Bll.Services;
using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Moq;
using Match = Core.Domain.Entities.Match;
using MatchType = Core.Domain.Enums.MatchType;

namespace BllTest.Services;

public class PaymentServiceTests
{
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();
    private readonly Mock<IBalanceDueRepository> _balanceDueRepository = new();
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly PaymentService _sut;

    private static readonly Member Member = new() { Id = 1, Matricule = "G1", Name = "N", FirstName = "F", MemberTypeId = MemberTypeSeed.GlobalId };

    public PaymentServiceTests()
    {
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Member);
        _balanceDueRepository.Setup(r => r.GetOutstandingByMemberIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _participationRepository.Setup(r => r.GetByMatchIdAsync(It.IsAny<int>())).ReturnsAsync([]);

        _sut = new PaymentService(
            _participationRepository.Object, _matchRepository.Object, _memberRepository.Object,
            _balanceDueRepository.Object, _paymentRepository.Object, _unitOfWork.Object, _timeProvider);
    }

    private static Match SomeMatch(MatchStatus status = MatchStatus.Open) => new()
    {
        Id = 1, CourtId = 5, OrganizerId = Member.Id, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Type = MatchType.Private, Status = status, TotalAmount = 60m
    };

    private static Participation SomeParticipation(Match match, ParticipationStatus status = ParticipationStatus.Reserved) => new()
    {
        Id = 10, MatchId = match.Id, MemberId = Member.Id, SeatNumber = 1, Status = status, AmountDue = 15m, Match = match
    };

    // --- PayParticipationAsync (RG-PAY-003/007/008) ---

    [Fact]
    public async Task PayParticipationAsync_HappyPath_CreatesPaymentAndMarksParticipationPaid()
    {
        var match = SomeMatch();
        var participation = SomeParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        var result = await _sut.PayParticipationAsync("G1", 10, new PayDto("CB"));

        _paymentRepository.Verify(r => r.AddAsync(It.Is<Payment>(p => p.MemberId == Member.Id && p.ParticipationId == 10 && p.Amount == 15m)), Times.Once);
        _participationRepository.Verify(r => r.Update(It.Is<Participation>(p => p.Status == ParticipationStatus.Paid && p.PaymentDate != null)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(15m, result.Amount);
    }

    [Fact]
    public async Task PayParticipationAsync_LastOfFourPlayers_CompletesMatch()
    {
        var match = SomeMatch();
        var participation = SomeParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);
        _participationRepository.Setup(r => r.GetByMatchIdAsync(1)).ReturnsAsync(
        [
            new Participation { Id = 11, MatchId = 1, SeatNumber = 2, Status = ParticipationStatus.Paid, AmountDue = 15m },
            new Participation { Id = 12, MatchId = 1, SeatNumber = 3, Status = ParticipationStatus.Paid, AmountDue = 15m },
            new Participation { Id = 13, MatchId = 1, SeatNumber = 4, Status = ParticipationStatus.Paid, AmountDue = 15m }
        ]);

        await _sut.PayParticipationAsync("G1", 10, new PayDto());

        _matchRepository.Verify(r => r.Update(It.Is<Match>(m => m.Status == MatchStatus.Complete)), Times.Once);
    }

    [Fact]
    public async Task PayParticipationAsync_NotLastPlayer_DoesNotCompleteMatch()
    {
        var match = SomeMatch();
        var participation = SomeParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);
        _participationRepository.Setup(r => r.GetByMatchIdAsync(1)).ReturnsAsync(
        [
            new Participation { Id = 11, MatchId = 1, SeatNumber = 2, Status = ParticipationStatus.Reserved, AmountDue = 15m }
        ]);

        await _sut.PayParticipationAsync("G1", 10, new PayDto());

        _matchRepository.Verify(r => r.Update(It.Is<Match>(m => m.Status != MatchStatus.Complete && m.AmountPaid == 15m)), Times.Once);
    }

    [Fact]
    public async Task PayParticipationAsync_OutstandingBalance_IsFoldedIntoPaymentAndSettled()
    {
        var match = SomeMatch();
        var participation = SomeParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);
        var balance = new BalanceDue { Id = 50, MemberId = Member.Id, MatchId = 99, Amount = 45m, Status = BalanceDueStatus.Due };
        _balanceDueRepository.Setup(r => r.GetOutstandingByMemberIdAsync(Member.Id)).ReturnsAsync([balance]);

        var result = await _sut.PayParticipationAsync("G1", 10, new PayDto());

        Assert.Equal(60m, result.Amount); // 15 (seat) + 45 (balance)
        Assert.Equal(50, result.BalanceDueId);
        _balanceDueRepository.Verify(r => r.Update(It.Is<BalanceDue>(s => s.Id == 50 && s.Status == BalanceDueStatus.Paid)), Times.Once);
    }

    [Fact]
    public async Task PayParticipationAsync_UnknownParticipation_ThrowsParticipationNotFoundException()
    {
        _participationRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Participation?)null);

        await Assert.ThrowsAsync<ParticipationNotFoundException>(() => _sut.PayParticipationAsync("G1", 999, new PayDto()));
    }

    [Fact]
    public async Task PayParticipationAsync_CallerDoesNotOwnParticipation_ThrowsParticipationNotFoundException()
    {
        var match = SomeMatch();
        var participation = SomeParticipation(match);
        participation.MemberId = 999;
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        await Assert.ThrowsAsync<ParticipationNotFoundException>(() => _sut.PayParticipationAsync("G1", 10, new PayDto()));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PayParticipationAsync_MatchAlreadyPlayed_ThrowsMatchNotModifiableException()
    {
        var match = SomeMatch();
        match.Date = new DateOnly(2026, 8, 1);
        var participation = SomeParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        await Assert.ThrowsAsync<MatchNotModifiableException>(() => _sut.PayParticipationAsync("G1", 10, new PayDto()));
    }

    [Fact]
    public async Task PayParticipationAsync_AlreadyPaid_ThrowsParticipationAlreadyPaidException()
    {
        var match = SomeMatch();
        var participation = SomeParticipation(match, ParticipationStatus.Paid);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        await Assert.ThrowsAsync<ParticipationAlreadyPaidException>(() => _sut.PayParticipationAsync("G1", 10, new PayDto()));
    }

    // --- PayBalanceDueAsync (RG-PAY-005/006) ---

    [Fact]
    public async Task PayBalanceDueAsync_HappyPath_CreatesPaymentAndSettlesBalance()
    {
        var balance = new BalanceDue { Id = 50, MemberId = Member.Id, MatchId = 1, Amount = 30m, Status = BalanceDueStatus.Due };
        _balanceDueRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(balance);

        var result = await _sut.PayBalanceDueAsync("G1", 50, new PayDto());

        Assert.Equal(30m, result.Amount);
        _balanceDueRepository.Verify(r => r.Update(It.Is<BalanceDue>(s => s.Status == BalanceDueStatus.Paid)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PayBalanceDueAsync_UnknownBalance_ThrowsBalanceDueNotFoundException()
    {
        _balanceDueRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((BalanceDue?)null);

        await Assert.ThrowsAsync<BalanceDueNotFoundException>(() => _sut.PayBalanceDueAsync("G1", 999, new PayDto()));
    }

    [Fact]
    public async Task PayBalanceDueAsync_CallerDoesNotOwnBalance_ThrowsBalanceDueNotFoundException()
    {
        var balance = new BalanceDue { Id = 50, MemberId = 999, MatchId = 1, Amount = 30m, Status = BalanceDueStatus.Due };
        _balanceDueRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(balance);

        await Assert.ThrowsAsync<BalanceDueNotFoundException>(() => _sut.PayBalanceDueAsync("G1", 50, new PayDto()));
    }

    [Fact]
    public async Task PayBalanceDueAsync_AlreadyPaid_ThrowsBalanceDueAlreadyPaidException()
    {
        var balance = new BalanceDue { Id = 50, MemberId = Member.Id, MatchId = 1, Amount = 30m, Status = BalanceDueStatus.Paid };
        _balanceDueRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(balance);

        await Assert.ThrowsAsync<BalanceDueAlreadyPaidException>(() => _sut.PayBalanceDueAsync("G1", 50, new PayDto()));
    }

    // --- GetMyUnpaidBalancesAsync ---

    [Fact]
    public async Task GetMyUnpaidBalancesAsync_ReturnsOutstandingBalancesForCaller()
    {
        _balanceDueRepository.Setup(r => r.GetOutstandingByMemberIdAsync(Member.Id)).ReturnsAsync(
        [
            new BalanceDue { Id = 50, MemberId = Member.Id, MatchId = 1, Amount = 30m, Status = BalanceDueStatus.Due }
        ]);

        var result = (await _sut.GetMyUnpaidBalancesAsync("G1")).ToList();

        Assert.Single(result);
        Assert.Equal(50, result[0].Id);
        Assert.Equal(30m, result[0].Amount);
    }

    [Fact]
    public async Task GetMyUnpaidBalancesAsync_UnknownMatricule_ThrowsMemberNotFoundByMatriculeException()
    {
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundByMatriculeException>(() => _sut.GetMyUnpaidBalancesAsync("G999"));
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
