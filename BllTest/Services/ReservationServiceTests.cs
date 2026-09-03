using Bll.Services;
using Core.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Match = Core.Domain.Entities.Match;

namespace BllTest.Services;

public class ReservationServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<ITerrainRepository> _terrainRepository = new();
    private readonly Mock<IMembreRepository> _membreRepository = new();
    private readonly Mock<IHoraireSiteRepository> _horaireSiteRepository = new();
    private readonly Mock<IJourFermetureRepository> _jourFermetureRepository = new();
    private readonly Mock<ISoldeDuRepository> _soldeDuRepository = new();
    private readonly Mock<IPenaliteRepository> _penaliteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreerReservationDto>> _createValidator = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly ReservationService _sut;

    private static readonly TypeMembre TypeGlobal = new()
    {
        Id = TypeMembreSeed.GlobalId, Code = TypeMembreSeed.GlobalCode, Libelle = "Membre global", PrefixeMatricule = "G", DelaiReservationJours = 21
    };
    private static readonly TypeMembre TypeSite = new()
    {
        Id = TypeMembreSeed.SiteId, Code = TypeMembreSeed.SiteCode, Libelle = "Membre de site", PrefixeMatricule = "S", DelaiReservationJours = 14
    };
    private static readonly Membre Organisateur = new()
    {
        Id = 10, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembreId = TypeMembreSeed.GlobalId, TypeMembre = TypeGlobal
    };
    private static readonly Terrain UnTerrain = new() { Id = 5, Name = "Court 1", SiteId = 1, Actif = true };
    private static readonly HoraireSite UnHoraire = new()
    {
        Id = 1, SiteId = 1, Annee = 2026, HeurePremiereReservation = new TimeOnly(10, 0), HeureDerniereReservation = new TimeOnly(20, 0),
        DureeMatchMinutes = 90, PauseMinutes = 15, PrixMatch = 60m
    };
    private static readonly CreerReservationDto ValidDto = new(5, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

    public ReservationServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreerReservationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Organisateur);
        _terrainRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(UnTerrain);
        _horaireSiteRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync(UnHoraire);
        _jourFermetureRepository.Setup(r => r.GetBySiteIdAsync(1)).ReturnsAsync([]);
        _jourFermetureRepository.Setup(r => r.GetGlobalAsync()).ReturnsAsync([]);
        _soldeDuRepository.Setup(r => r.GetOutstandingByMembreIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _penaliteRepository.Setup(r => r.GetActiveByMembreIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _participationRepository.Setup(r => r.GetActiveByMembreIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _matchRepository.Setup(r => r.GetByTerrainAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync([]);

        _sut = new ReservationService(
            _matchRepository.Object, _participationRepository.Object, _terrainRepository.Object, _membreRepository.Object,
            _horaireSiteRepository.Object, _jourFermetureRepository.Object, _soldeDuRepository.Object, _penaliteRepository.Object,
            _unitOfWork.Object, _timeProvider, _createValidator.Object);
    }

    [Fact]
    public async Task CreerReservationAsync_HappyPath_CreatesPrivateMatchWithOrganizerAsParticipant_AndSavesOnce()
    {
        var result = await _sut.CreerReservationAsync("G1", ValidDto);

        _matchRepository.Verify(r => r.AddAsync(It.Is<Match>(m =>
            m.TerrainId == 5 && m.Date == ValidDto.Date && m.StartTime == ValidDto.StartTime &&
            m.TypeMatch == TypeMatch.Private && m.Statut == StatutMatch.Open &&
            m.OrganisateurId == Organisateur.Id && m.MontantTotal == 60m)), Times.Once);

        _participationRepository.Verify(r => r.AddAsync(It.Is<Participation>(p =>
            p.MembreId == Organisateur.Id && p.MontantDu == 15m &&
            p.NumeroPlace == 1 && p.Role == RoleParticipation.Organisateur && p.Statut == StatutParticipation.Reservee)), Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal("Private", result.TypeMatch);
        Assert.Equal("Open", result.Statut);
        Assert.Equal(Organisateur.Id, result.OrganisateurId);
        Assert.Equal(60m, result.MontantTotal);
    }

    [Fact]
    public async Task CreerReservationAsync_EstPublicTrue_CreatesPublicMatch()
    {
        var dto = ValidDto with { EstPublic = true };

        var result = await _sut.CreerReservationAsync("G1", dto);

        Assert.Equal("Public", result.TypeMatch);
    }

    [Fact]
    public async Task CreerReservationAsync_UnknownMatricule_ThrowsMembreNotFoundByMatriculeException_AndNeverSaves()
    {
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundByMatriculeException>(() => _sut.CreerReservationAsync("G999", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_UnknownTerrain_ThrowsTerrainNotFoundException_AndNeverSaves()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Terrain?)null);
        var dto = ValidDto with { TerrainId = 999 };

        await Assert.ThrowsAsync<TerrainNotFoundException>(() => _sut.CreerReservationAsync("G1", dto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_TerrainInactif_ThrowsTerrainInactifException_AndNeverSaves()
    {
        _terrainRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Terrain { Id = 5, Name = "Court 1", SiteId = 1, Actif = false });

        await Assert.ThrowsAsync<TerrainInactifException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_SiteMemberOnOtherSite_ThrowsSiteNonAutoriseException_AndNeverSaves()
    {
        var membreSite = new Membre { Id = 20, Matricule = "S1", Name = "N", FirstName = "F", TypeMembreId = TypeMembreSeed.SiteId, TypeMembre = TypeSite, SiteId = 999 };
        _membreRepository.Setup(r => r.GetByMatriculeAsync("S1")).ReturnsAsync(membreSite);

        await Assert.ThrowsAsync<SiteNonAutoriseException>(() => _sut.CreerReservationAsync("S1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_SiteMemberOnOwnSite_Succeeds()
    {
        var membreSite = new Membre { Id = 20, Matricule = "S1", Name = "N", FirstName = "F", TypeMembreId = TypeMembreSeed.SiteId, TypeMembre = TypeSite, SiteId = 1 };
        _membreRepository.Setup(r => r.GetByMatriculeAsync("S1")).ReturnsAsync(membreSite);

        var result = await _sut.CreerReservationAsync("S1", ValidDto);

        Assert.Equal(20, result.OrganisateurId);
    }

    [Fact]
    public async Task CreerReservationAsync_OutsideBookingWindow_ThrowsDelaiReservationNonRespecteException_AndNeverSaves()
    {
        // Global member: 21-day window. Match is 30 days out from "today" (2026-09-01) — too early.
        var dto = ValidDto with { Date = new DateOnly(2026, 10, 1) };

        await Assert.ThrowsAsync<DelaiReservationNonRespecteException>(() => _sut.CreerReservationAsync("G1", dto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_SoldeDuImpaye_ThrowsSoldeDuException_AndNeverSaves()
    {
        _soldeDuRepository.Setup(r => r.GetOutstandingByMembreIdAsync(Organisateur.Id))
            .ReturnsAsync([new SoldeDu { MembreId = Organisateur.Id, MatchId = 1, Statut = StatutSoldeDu.Du, Montant = 15m }]);

        await Assert.ThrowsAsync<SoldeDuException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_PenaliteActive_ThrowsPenaliteActiveException_AndNeverSaves()
    {
        _penaliteRepository.Setup(r => r.GetActiveByMembreIdAsync(Organisateur.Id))
            .ReturnsAsync([new Penalite { MembreId = Organisateur.Id, DateDebut = new DateOnly(2026, 8, 30), DateFin = new DateOnly(2026, 9, 6), Active = true }]);

        await Assert.ThrowsAsync<PenaliteActiveException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_NoHoraireDefined_ThrowsHorairesSiteNonDefinisException_AndNeverSaves()
    {
        _horaireSiteRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync((HoraireSite?)null);

        await Assert.ThrowsAsync<HorairesSiteNonDefinisException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_StartTimeNotAGeneratedSlot_ThrowsCreneauHorsHorairesException_AndNeverSaves()
    {
        var dto = ValidDto with { StartTime = new TimeOnly(10, 5) };

        await Assert.ThrowsAsync<CreneauHorsHorairesException>(() => _sut.CreerReservationAsync("G1", dto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_ClosureDay_ThrowsJourFermeException_AndNeverSaves()
    {
        _jourFermetureRepository.Setup(r => r.GetBySiteIdAsync(1))
            .ReturnsAsync([new JourFermeture { SiteId = 1, DateFermeture = ValidDto.Date }]);

        await Assert.ThrowsAsync<JourFermeException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_GlobalClosureDay_ThrowsJourFermeException_AndNeverSaves()
    {
        _jourFermetureRepository.Setup(r => r.GetGlobalAsync())
            .ReturnsAsync([new JourFermeture { SiteId = null, DateFermeture = ValidDto.Date }]);

        await Assert.ThrowsAsync<JourFermeException>(() => _sut.CreerReservationAsync("G1", ValidDto));
    }

    [Fact]
    public async Task CreerReservationAsync_ExactDoubleBooking_ThrowsCreneauIndisponibleException_AndNeverSaves()
    {
        _matchRepository.Setup(r => r.GetByTerrainAndDateAsync(5, ValidDto.Date)).ReturnsAsync(
        [
            new Match { TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30), TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 99 }
        ]);

        await Assert.ThrowsAsync<CreneauIndisponibleException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _matchRepository.Verify(r => r.AddAsync(It.IsAny<Match>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_DifferentStartTimeSameTerrainAndDate_Succeeds()
    {
        _matchRepository.Setup(r => r.GetByTerrainAndDateAsync(5, ValidDto.Date)).ReturnsAsync(
        [
            new Match { TerrainId = 5, Date = ValidDto.Date, StartTime = new TimeOnly(11, 45), EndTime = new TimeOnly(13, 15), TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 99 }
        ]);

        var result = await _sut.CreerReservationAsync("G1", ValidDto);

        Assert.Equal(ValidDto.StartTime, result.StartTime);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreerReservationAsync_OverlappingParticipationOnAnotherMatch_ThrowsChevauchementMatchException_AndNeverSaves()
    {
        _participationRepository.Setup(r => r.GetActiveByMembreIdAsync(Organisateur.Id)).ReturnsAsync(
        [
            new Participation
            {
                MatchId = 777, NumeroPlace = 1, Statut = StatutParticipation.Reservee,
                Match = new Match { Id = 777, TerrainId = 6, OrganisateurId = 1, Date = ValidDto.Date, StartTime = new TimeOnly(10, 30), EndTime = new TimeOnly(12, 0), TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open }
            }
        ]);

        await Assert.ThrowsAsync<ChevauchementMatchException>(() => _sut.CreerReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreerReservationAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = ValidDto with { TerrainId = 0 };
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreerReservationDto.TerrainId), "'Terrain Id' must be greater than '0'.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreerReservationAsync("G1", dto));

        _matchRepository.Verify(r => r.AddAsync(It.IsAny<Match>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetReservationByIdAsync_ViewedByOrganizer_ReturnsDto()
    {
        var match = new Match
        {
            Id = 1, TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30),
            TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = Organisateur.Id,
            Participations = [new Participation { MatchId = 1, MembreId = Organisateur.Id, NumeroPlace = 1, Statut = StatutParticipation.Reservee }]
        };
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);

        var result = await _sut.GetReservationByIdAsync("G1", 1);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetReservationByIdAsync_PrivateMatchViewedByNonParticipant_ThrowsMatchNotFoundException()
    {
        var match = new Match
        {
            Id = 1, TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30),
            TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = Organisateur.Id,
            Participations = [new Participation { MatchId = 1, MembreId = Organisateur.Id, NumeroPlace = 1, Statut = StatutParticipation.Reservee }]
        };
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _membreRepository.Setup(r => r.GetByMatriculeAsync("L1")).ReturnsAsync(new Membre { Id = 30, Matricule = "L1", Name = "N", FirstName = "F", TypeMembreId = 3 });

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.GetReservationByIdAsync("L1", 1));
    }

    [Fact]
    public async Task GetReservationByIdAsync_PublicMatchViewedByAnyone_ReturnsDto()
    {
        var match = new Match
        {
            Id = 1, TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30),
            TypeMatch = TypeMatch.Public, Statut = StatutMatch.Open, OrganisateurId = Organisateur.Id
        };
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _membreRepository.Setup(r => r.GetByMatriculeAsync("L1")).ReturnsAsync((Membre?)null);

        var result = await _sut.GetReservationByIdAsync("L1", 1);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetReservationByIdAsync_UnknownId_ThrowsMatchNotFoundException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Match?)null);

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.GetReservationByIdAsync("G1", 999));
    }

    [Fact]
    public async Task GetMyReservationsAsync_ExistingMembre_ReturnsOwnMatchesAsDtos()
    {
        _matchRepository.Setup(r => r.GetByOrganisateurIdAsync(Organisateur.Id)).ReturnsAsync(
        [
            new Match { Id = 1, TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = Organisateur.Id }
        ]);

        var result = await _sut.GetMyReservationsAsync("G1");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetMyReservationsAsync_UnknownMatricule_ThrowsMembreNotFoundByMatriculeException()
    {
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundByMatriculeException>(() => _sut.GetMyReservationsAsync("G999"));
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
