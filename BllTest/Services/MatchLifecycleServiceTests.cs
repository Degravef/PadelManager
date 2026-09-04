using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Moq;
using Match = Core.Domain.Entities.Match;
using MatchType = Core.Domain.Enums.MatchType;

namespace BllTest.Services;

public class MatchLifecycleServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<IPenaltyRepository> _penaltyRepository = new();
    private readonly Mock<IBalanceDueRepository> _balanceDueRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero));
    private readonly MatchLifecycleService _sut;

    private static readonly DateOnly Today = new(2026, 9, 1);
    private static readonly DateOnly Tomorrow = new(2026, 9, 2);

    public MatchLifecycleServiceTests()
    {
        _balanceDueRepository.Setup(r => r.GetByMatchIdAsync(It.IsAny<int>())).ReturnsAsync((BalanceDue?)null);

        _sut = new MatchLifecycleService(
            _matchRepository.Object, _participationRepository.Object, _penaltyRepository.Object,
            _balanceDueRepository.Object, _unitOfWork.Object, _timeProvider);
    }

    private static Match Match(int id, MatchType type, params Participation[] participations) => new()
    {
        Id = id, CourtId = 1, OrganizerId = 1, Date = Tomorrow, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30),
        Type = type, Status = MatchStatus.Open, TotalAmount = 60m, Participations = participations.ToList()
    };

    private static Participation P(ParticipationStatus status, decimal amount = 15m) => new() { MatchId = 0, SeatNumber = 1, Status = status, AmountDue = amount };

    [Fact]
    public async Task ExecuteDailyBatchAsync_PrivateMatchIncompleteRoster_SwitchesToPublicAndPenalizesOrganizer()
    {
        var match = Match(1, MatchType.Private, P(ParticipationStatus.Reserved), P(ParticipationStatus.Reserved));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        Assert.Equal(MatchType.Public, match.Type);
        Assert.NotNull(match.PublicSwitchDate);
        _matchRepository.Verify(r => r.Update(match), Times.AtLeastOnce);
        _penaltyRepository.Verify(r => r.AddAsync(It.Is<Penalty>(p => p.MemberId == match.OrganizerId && p.MatchId == 1)), Times.Once);
        Assert.Equal(1, result.MatchesSwitchedIncompleteRoster);
        Assert.Equal(1, result.PenaltiesApplied);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_PrivateMatchFourRegisteredOneUnpaid_FreesSeatAndSwitchesToPublicWithoutPenalty()
    {
        var unpaid = P(ParticipationStatus.Reserved);
        var match = Match(1, MatchType.Private, P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), unpaid);
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        Assert.Equal(MatchType.Public, match.Type);
        Assert.DoesNotContain(unpaid, match.Participations);
        _participationRepository.Verify(r => r.Delete(unpaid), Times.Once);
        _penaltyRepository.Verify(r => r.AddAsync(It.IsAny<Penalty>()), Times.Never);
        Assert.Equal(1, result.MatchesSwitchedUnpaidSeat);
        Assert.Equal(0, result.PenaltiesApplied);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_PrivateMatchFourPaid_StaysPrivateUnchanged()
    {
        var match = Match(1, MatchType.Private, P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        Assert.Equal(MatchType.Private, match.Type);
        _matchRepository.Verify(r => r.Update(It.IsAny<Match>()), Times.Never);
        _balanceDueRepository.Verify(r => r.AddAsync(It.IsAny<BalanceDue>()), Times.Never);
        Assert.Equal(0, result.MatchesSwitchedIncompleteRoster + result.MatchesSwitchedUnpaidSeat);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_PublicMatchIncomplete_CreatesBalanceDueForOrganizer()
    {
        var match = Match(1, MatchType.Public, P(ParticipationStatus.Paid), P(ParticipationStatus.Paid));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        _balanceDueRepository.Verify(r => r.AddAsync(It.Is<BalanceDue>(s => s.MemberId == match.OrganizerId && s.MatchId == 1 && s.Amount == 30m)), Times.Once);
        Assert.Equal(1, result.BalancesDueCreated);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_PublicMatchAlreadyHasBalanceDue_DoesNotCreateDuplicate()
    {
        var match = Match(1, MatchType.Public, P(ParticipationStatus.Paid));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);
        _balanceDueRepository.Setup(r => r.GetByMatchIdAsync(1)).ReturnsAsync(new BalanceDue { MemberId = 1, MatchId = 1, Amount = 45m, Status = BalanceDueStatus.Due });

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        _balanceDueRepository.Verify(r => r.AddAsync(It.IsAny<BalanceDue>()), Times.Never);
        Assert.Equal(0, result.BalancesDueCreated);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_PublicMatchFullyPaid_MarksMatchComplete()
    {
        var match = Match(1, MatchType.Public, P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        Assert.Equal(MatchStatus.Complete, match.Status);
        Assert.Equal(1, result.MatchesCompleted);
        _balanceDueRepository.Verify(r => r.AddAsync(It.IsAny<BalanceDue>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_CancelledMatch_IsSkippedEntirely()
    {
        var match = Match(1, MatchType.Private, P(ParticipationStatus.Reserved));
        match.Status = MatchStatus.Cancelled;
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        _matchRepository.Verify(r => r.Update(It.IsAny<Match>()), Times.Never);
        _penaltyRepository.Verify(r => r.AddAsync(It.IsAny<Penalty>()), Times.Never);
        Assert.Equal(0, result.MatchesSwitchedIncompleteRoster);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_ProcessesTomorrowRelativeToGivenDate()
    {
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([]);

        var result = await _sut.ExecuteDailyBatchAsync(Today);

        _matchRepository.Verify(r => r.GetByDateAsync(Tomorrow), Times.Once);
        Assert.Equal(Tomorrow, result.ProcessedDate);
    }

    [Fact]
    public async Task ExecuteDailyBatchAsync_MultipleMatches_SavesExactlyOnce()
    {
        var match1 = Match(1, MatchType.Private, P(ParticipationStatus.Reserved));
        var match2 = Match(2, MatchType.Public, P(ParticipationStatus.Paid));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match1, match2]);

        await _sut.ExecuteDailyBatchAsync(Today);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
