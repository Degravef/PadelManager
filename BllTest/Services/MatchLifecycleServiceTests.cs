using Bll.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Moq;
using Match = Core.Domain.Entities.Match;

namespace BllTest.Services;

public class MatchLifecycleServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<IPenaliteRepository> _penaliteRepository = new();
    private readonly Mock<ISoldeDuRepository> _soldeDuRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero));
    private readonly MatchLifecycleService _sut;

    private static readonly DateOnly Today = new(2026, 9, 1);
    private static readonly DateOnly Tomorrow = new(2026, 9, 2);

    public MatchLifecycleServiceTests()
    {
        _soldeDuRepository.Setup(r => r.GetByMatchIdAsync(It.IsAny<int>())).ReturnsAsync((SoldeDu?)null);

        _sut = new MatchLifecycleService(
            _matchRepository.Object, _participationRepository.Object, _penaliteRepository.Object,
            _soldeDuRepository.Object, _unitOfWork.Object, _timeProvider);
    }

    private static Match Match(int id, TypeMatch type, params Participation[] participations) => new()
    {
        Id = id, TerrainId = 1, OrganisateurId = 1, Date = Tomorrow, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30),
        TypeMatch = type, Statut = StatutMatch.Open, MontantTotal = 60m, Participations = participations.ToList()
    };

    private static Participation P(StatutParticipation statut, decimal montant = 15m) => new() { MatchId = 0, NumeroPlace = 1, Statut = statut, MontantDu = montant };

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_PrivateMatchIncompleteRoster_SwitchesToPublicAndPenalizesOrganizer()
    {
        var match = Match(1, TypeMatch.Private, P(StatutParticipation.Reservee), P(StatutParticipation.Reservee));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        Assert.Equal(TypeMatch.Public, match.TypeMatch);
        Assert.NotNull(match.DateBasculePublic);
        _matchRepository.Verify(r => r.Update(match), Times.AtLeastOnce);
        _penaliteRepository.Verify(r => r.AddAsync(It.Is<Penalite>(p => p.MembreId == match.OrganisateurId && p.MatchId == 1)), Times.Once);
        Assert.Equal(1, result.MatchesBasculesEffectifIncomplet);
        Assert.Equal(1, result.PenalitesAppliquees);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_PrivateMatchFourRegisteredOneUnpaid_FreesSeatAndSwitchesToPublicWithoutPenalty()
    {
        var unpaid = P(StatutParticipation.Reservee);
        var match = Match(1, TypeMatch.Private, P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee), unpaid);
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        Assert.Equal(TypeMatch.Public, match.TypeMatch);
        Assert.DoesNotContain(unpaid, match.Participations);
        _participationRepository.Verify(r => r.Delete(unpaid), Times.Once);
        _penaliteRepository.Verify(r => r.AddAsync(It.IsAny<Penalite>()), Times.Never);
        Assert.Equal(1, result.MatchesBasculesPaiementManquant);
        Assert.Equal(0, result.PenalitesAppliquees);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_PrivateMatchFourPaid_StaysPrivateUnchanged()
    {
        var match = Match(1, TypeMatch.Private, P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        Assert.Equal(TypeMatch.Private, match.TypeMatch);
        _matchRepository.Verify(r => r.Update(It.IsAny<Match>()), Times.Never);
        _soldeDuRepository.Verify(r => r.AddAsync(It.IsAny<SoldeDu>()), Times.Never);
        Assert.Equal(0, result.MatchesBasculesEffectifIncomplet + result.MatchesBasculesPaiementManquant);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_PublicMatchIncomplete_CreatesSoldeDuForOrganizer()
    {
        var match = Match(1, TypeMatch.Public, P(StatutParticipation.Payee), P(StatutParticipation.Payee));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        _soldeDuRepository.Verify(r => r.AddAsync(It.Is<SoldeDu>(s => s.MembreId == match.OrganisateurId && s.MatchId == 1 && s.Montant == 30m)), Times.Once);
        Assert.Equal(1, result.SoldesCrees);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_PublicMatchAlreadyHasSoldeDu_DoesNotCreateDuplicate()
    {
        var match = Match(1, TypeMatch.Public, P(StatutParticipation.Payee));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);
        _soldeDuRepository.Setup(r => r.GetByMatchIdAsync(1)).ReturnsAsync(new SoldeDu { MembreId = 1, MatchId = 1, Montant = 45m, Statut = StatutSoldeDu.Du });

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        _soldeDuRepository.Verify(r => r.AddAsync(It.IsAny<SoldeDu>()), Times.Never);
        Assert.Equal(0, result.SoldesCrees);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_PublicMatchFullyPaid_MarksMatchComplete()
    {
        var match = Match(1, TypeMatch.Public, P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        Assert.Equal(StatutMatch.Complete, match.Statut);
        Assert.Equal(1, result.MatchesCompletes);
        _soldeDuRepository.Verify(r => r.AddAsync(It.IsAny<SoldeDu>()), Times.Never);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_CancelledMatch_IsSkippedEntirely()
    {
        var match = Match(1, TypeMatch.Private, P(StatutParticipation.Reservee));
        match.Statut = StatutMatch.Cancelled;
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        _matchRepository.Verify(r => r.Update(It.IsAny<Match>()), Times.Never);
        _penaliteRepository.Verify(r => r.AddAsync(It.IsAny<Penalite>()), Times.Never);
        Assert.Equal(0, result.MatchesBasculesEffectifIncomplet);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_ProcessesTomorrowRelativeToGivenDate()
    {
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([]);

        var result = await _sut.ExecuterTraitementQuotidienAsync(Today);

        _matchRepository.Verify(r => r.GetByDateAsync(Tomorrow), Times.Once);
        Assert.Equal(Tomorrow, result.DateTraitee);
    }

    [Fact]
    public async Task ExecuterTraitementQuotidienAsync_MultipleMatches_SavesExactlyOnce()
    {
        var match1 = Match(1, TypeMatch.Private, P(StatutParticipation.Reservee));
        var match2 = Match(2, TypeMatch.Public, P(StatutParticipation.Payee));
        _matchRepository.Setup(r => r.GetByDateAsync(Tomorrow)).ReturnsAsync([match1, match2]);

        await _sut.ExecuterTraitementQuotidienAsync(Today);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
