using Bll.Services;
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
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreerReservationDto>> _createValidator = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly ReservationService _sut;

    private static readonly Membre Organisateur = new() { Id = 10, Matricule = "G1", Name = "Doe", FirstName = "Jane", TypeMembre = TypeMembre.Global };
    private static readonly Terrain UnTerrain = new() { Id = 5, Name = "Court 1", SiteId = 1 };
    private static readonly CreerReservationDto ValidDto = new(5, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

    public ReservationServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreerReservationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Organisateur);
        _terrainRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(UnTerrain);
        _matchRepository.Setup(r => r.GetByTerrainAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync([]);

        _sut = new ReservationService(_matchRepository.Object, _participationRepository.Object,
            _terrainRepository.Object, _membreRepository.Object, _unitOfWork.Object, _timeProvider, _createValidator.Object);
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
            p.MembreId == Organisateur.Id && p.MontantDu == 15m && !p.APaye)), Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal("Private", result.TypeMatch);
        Assert.Equal("Open", result.Statut);
        Assert.Equal(Organisateur.Id, result.OrganisateurId);
        Assert.Equal(60m, result.MontantTotal);
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
    public async Task CreerReservationAsync_ExactDoubleBooking_ThrowsCreneauIndisponibleException_AndNeverSaves()
    {
        _matchRepository.Setup(r => r.GetByTerrainAndDateAsync(5, ValidDto.Date)).ReturnsAsync(
        [
            new Match { TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 99 }
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
            new Match { TerrainId = 5, Date = ValidDto.Date, StartTime = new TimeOnly(14, 0), TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 99 }
        ]);

        var result = await _sut.CreerReservationAsync("G1", ValidDto);

        Assert.Equal(ValidDto.StartTime, result.StartTime);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task GetReservationByIdAsync_ExistingMatch_ReturnsDto()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(
            new Match { Id = 1, TerrainId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open, OrganisateurId = 10 });

        var result = await _sut.GetReservationByIdAsync(1);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetReservationByIdAsync_UnknownId_ThrowsMatchNotFoundException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Match?)null);

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.GetReservationByIdAsync(999));
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
