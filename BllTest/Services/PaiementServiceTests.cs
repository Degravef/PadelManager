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

namespace BllTest.Services;

public class PaiementServiceTests
{
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IMembreRepository> _membreRepository = new();
    private readonly Mock<ISoldeDuRepository> _soldeDuRepository = new();
    private readonly Mock<IPaiementRepository> _paiementRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly PaiementService _sut;

    private static readonly Membre Membre = new() { Id = 1, Matricule = "G1", Name = "N", FirstName = "F", TypeMembreId = TypeMembreSeed.GlobalId };

    public PaiementServiceTests()
    {
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Membre);
        _soldeDuRepository.Setup(r => r.GetOutstandingByMembreIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _participationRepository.Setup(r => r.GetByMatchIdAsync(It.IsAny<int>())).ReturnsAsync([]);

        _sut = new PaiementService(
            _participationRepository.Object, _matchRepository.Object, _membreRepository.Object,
            _soldeDuRepository.Object, _paiementRepository.Object, _unitOfWork.Object, _timeProvider);
    }

    private static Match UnMatch(StatutMatch statut = StatutMatch.Open) => new()
    {
        Id = 1, TerrainId = 5, OrganisateurId = Membre.Id, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), TypeMatch = TypeMatch.Private, Statut = statut, MontantTotal = 60m
    };

    private static Participation UneParticipation(Match match, StatutParticipation statut = StatutParticipation.Reservee) => new()
    {
        Id = 10, MatchId = match.Id, MembreId = Membre.Id, NumeroPlace = 1, Statut = statut, MontantDu = 15m, Match = match
    };

    // --- PayerParticipationAsync (RG-PAY-003/007/008) ---

    [Fact]
    public async Task PayerParticipationAsync_HappyPath_CreatesPaiementAndMarksParticipationPayee()
    {
        var match = UnMatch();
        var participation = UneParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        var result = await _sut.PayerParticipationAsync("G1", 10, new PayerDto("CB"));

        _paiementRepository.Verify(r => r.AddAsync(It.Is<Paiement>(p => p.MembreId == Membre.Id && p.ParticipationId == 10 && p.Montant == 15m)), Times.Once);
        _participationRepository.Verify(r => r.Update(It.Is<Participation>(p => p.Statut == StatutParticipation.Payee && p.DateValidation != null)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(15m, result.Montant);
    }

    [Fact]
    public async Task PayerParticipationAsync_LastOfFourPlayers_CompletesMatch()
    {
        var match = UnMatch();
        var participation = UneParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);
        _participationRepository.Setup(r => r.GetByMatchIdAsync(1)).ReturnsAsync(
        [
            new Participation { Id = 11, MatchId = 1, NumeroPlace = 2, Statut = StatutParticipation.Payee, MontantDu = 15m },
            new Participation { Id = 12, MatchId = 1, NumeroPlace = 3, Statut = StatutParticipation.Payee, MontantDu = 15m },
            new Participation { Id = 13, MatchId = 1, NumeroPlace = 4, Statut = StatutParticipation.Payee, MontantDu = 15m }
        ]);

        await _sut.PayerParticipationAsync("G1", 10, new PayerDto());

        _matchRepository.Verify(r => r.Update(It.Is<Match>(m => m.Statut == StatutMatch.Complete)), Times.Once);
    }

    [Fact]
    public async Task PayerParticipationAsync_NotLastPlayer_DoesNotCompleteMatch()
    {
        var match = UnMatch();
        var participation = UneParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);
        _participationRepository.Setup(r => r.GetByMatchIdAsync(1)).ReturnsAsync(
        [
            new Participation { Id = 11, MatchId = 1, NumeroPlace = 2, Statut = StatutParticipation.Reservee, MontantDu = 15m }
        ]);

        await _sut.PayerParticipationAsync("G1", 10, new PayerDto());

        _matchRepository.Verify(r => r.Update(It.Is<Match>(m => m.Statut != StatutMatch.Complete && m.MontantPaye == 15m)), Times.Once);
    }

    [Fact]
    public async Task PayerParticipationAsync_OutstandingSolde_IsFoldedIntoPaymentAndSettled()
    {
        var match = UnMatch();
        var participation = UneParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);
        var solde = new SoldeDu { Id = 50, MembreId = Membre.Id, MatchId = 99, Montant = 45m, Statut = StatutSoldeDu.Du };
        _soldeDuRepository.Setup(r => r.GetOutstandingByMembreIdAsync(Membre.Id)).ReturnsAsync([solde]);

        var result = await _sut.PayerParticipationAsync("G1", 10, new PayerDto());

        Assert.Equal(60m, result.Montant); // 15 (seat) + 45 (solde)
        Assert.Equal(50, result.SoldeDuId);
        _soldeDuRepository.Verify(r => r.Update(It.Is<SoldeDu>(s => s.Id == 50 && s.Statut == StatutSoldeDu.Paye)), Times.Once);
    }

    [Fact]
    public async Task PayerParticipationAsync_UnknownParticipation_ThrowsParticipationNotFoundException()
    {
        _participationRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Participation?)null);

        await Assert.ThrowsAsync<ParticipationNotFoundException>(() => _sut.PayerParticipationAsync("G1", 999, new PayerDto()));
    }

    [Fact]
    public async Task PayerParticipationAsync_CallerDoesNotOwnParticipation_ThrowsParticipationNotFoundException()
    {
        var match = UnMatch();
        var participation = UneParticipation(match);
        participation.MembreId = 999;
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        await Assert.ThrowsAsync<ParticipationNotFoundException>(() => _sut.PayerParticipationAsync("G1", 10, new PayerDto()));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PayerParticipationAsync_MatchAlreadyPlayed_ThrowsMatchNonModifiableException()
    {
        var match = UnMatch();
        match.Date = new DateOnly(2026, 8, 1);
        var participation = UneParticipation(match);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        await Assert.ThrowsAsync<MatchNonModifiableException>(() => _sut.PayerParticipationAsync("G1", 10, new PayerDto()));
    }

    [Fact]
    public async Task PayerParticipationAsync_AlreadyPaid_ThrowsParticipationDejaPayeeException()
    {
        var match = UnMatch();
        var participation = UneParticipation(match, StatutParticipation.Payee);
        _participationRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(participation);

        await Assert.ThrowsAsync<ParticipationDejaPayeeException>(() => _sut.PayerParticipationAsync("G1", 10, new PayerDto()));
    }

    // --- PayerSoldeAsync (RG-PAY-005/006) ---

    [Fact]
    public async Task PayerSoldeAsync_HappyPath_CreatesPaiementAndSettlesSolde()
    {
        var solde = new SoldeDu { Id = 50, MembreId = Membre.Id, MatchId = 1, Montant = 30m, Statut = StatutSoldeDu.Du };
        _soldeDuRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(solde);

        var result = await _sut.PayerSoldeAsync("G1", 50, new PayerDto());

        Assert.Equal(30m, result.Montant);
        _soldeDuRepository.Verify(r => r.Update(It.Is<SoldeDu>(s => s.Statut == StatutSoldeDu.Paye)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PayerSoldeAsync_UnknownSolde_ThrowsSoldeDuNotFoundException()
    {
        _soldeDuRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((SoldeDu?)null);

        await Assert.ThrowsAsync<SoldeDuNotFoundException>(() => _sut.PayerSoldeAsync("G1", 999, new PayerDto()));
    }

    [Fact]
    public async Task PayerSoldeAsync_CallerDoesNotOwnSolde_ThrowsSoldeDuNotFoundException()
    {
        var solde = new SoldeDu { Id = 50, MembreId = 999, MatchId = 1, Montant = 30m, Statut = StatutSoldeDu.Du };
        _soldeDuRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(solde);

        await Assert.ThrowsAsync<SoldeDuNotFoundException>(() => _sut.PayerSoldeAsync("G1", 50, new PayerDto()));
    }

    [Fact]
    public async Task PayerSoldeAsync_AlreadyPaid_ThrowsSoldeDuDejaPayeException()
    {
        var solde = new SoldeDu { Id = 50, MembreId = Membre.Id, MatchId = 1, Montant = 30m, Statut = StatutSoldeDu.Paye };
        _soldeDuRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(solde);

        await Assert.ThrowsAsync<SoldeDuDejaPayeException>(() => _sut.PayerSoldeAsync("G1", 50, new PayerDto()));
    }

    // --- GetMesSoldesImpayesAsync ---

    [Fact]
    public async Task GetMesSoldesImpayesAsync_ReturnsOutstandingSoldesForCaller()
    {
        _soldeDuRepository.Setup(r => r.GetOutstandingByMembreIdAsync(Membre.Id)).ReturnsAsync(
        [
            new SoldeDu { Id = 50, MembreId = Membre.Id, MatchId = 1, Montant = 30m, Statut = StatutSoldeDu.Du }
        ]);

        var result = (await _sut.GetMesSoldesImpayesAsync("G1")).ToList();

        Assert.Single(result);
        Assert.Equal(50, result[0].Id);
        Assert.Equal(30m, result[0].Montant);
    }

    [Fact]
    public async Task GetMesSoldesImpayesAsync_UnknownMatricule_ThrowsMembreNotFoundByMatriculeException()
    {
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundByMatriculeException>(() => _sut.GetMesSoldesImpayesAsync("G999"));
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
