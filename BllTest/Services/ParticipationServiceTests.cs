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

public class ParticipationServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<ITerrainRepository> _terrainRepository = new();
    private readonly Mock<IMembreRepository> _membreRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<AjouterJoueurDto>> _ajouterJoueurValidator = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly ParticipationService _sut;

    private static readonly TypeMembre TypeGlobal = new()
    {
        Id = TypeMembreSeed.GlobalId, Code = TypeMembreSeed.GlobalCode, Libelle = "Membre global", PrefixeMatricule = "G", DelaiReservationJours = 21
    };
    private static readonly Membre Organisateur = new() { Id = 1, Matricule = "G1", Name = "N", FirstName = "F", TypeMembreId = TypeMembreSeed.GlobalId, TypeMembre = TypeGlobal };
    private static readonly Membre Joueur = new() { Id = 2, Matricule = "G2", Name = "N2", FirstName = "F2", TypeMembreId = TypeMembreSeed.GlobalId, TypeMembre = TypeGlobal };
    private static readonly Terrain UnTerrain = new() { Id = 5, Name = "Court 1", SiteId = 1 };

    public ParticipationServiceTests()
    {
        _ajouterJoueurValidator.Setup(v => v.ValidateAsync(It.IsAny<AjouterJoueurDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Organisateur);
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G2")).ReturnsAsync(Joueur);
        _terrainRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(UnTerrain);
        _participationRepository.Setup(r => r.GetActiveByMembreIdAsync(It.IsAny<int>())).ReturnsAsync([]);

        _sut = new ParticipationService(
            _matchRepository.Object, _participationRepository.Object, _terrainRepository.Object, _membreRepository.Object,
            _unitOfWork.Object, _timeProvider, _ajouterJoueurValidator.Object);
    }

    private static Match PrivateMatch(int nbActifs = 1, StatutMatch statut = StatutMatch.Open) => new()
    {
        Id = 1, TerrainId = 5, OrganisateurId = Organisateur.Id, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), TypeMatch = TypeMatch.Private, Statut = statut,
        MontantTotal = 60m,
        Participations = Enumerable.Range(1, nbActifs)
            .Select(n => new Participation { MatchId = 1, MembreId = 100 + n, NumeroPlace = n, Statut = StatutParticipation.Reservee })
            .ToList()
    };

    private static Match PublicMatch(int nbActifs = 1, StatutMatch statut = StatutMatch.Open) => new()
    {
        Id = 2, TerrainId = 5, OrganisateurId = Organisateur.Id, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), TypeMatch = TypeMatch.Public, Statut = statut,
        MontantTotal = 60m,
        Participations = Enumerable.Range(1, nbActifs)
            .Select(n => new Participation { MatchId = 2, MembreId = 100 + n, NumeroPlace = n, Statut = StatutParticipation.Reservee })
            .ToList()
    };

    // --- AjouterJoueurMatchPriveAsync (RG-PRV-001/002) ---

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_HappyPath_AddsPlayerAndSaves()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());

        var result = await _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("G2"));

        _participationRepository.Verify(r => r.AddAsync(It.Is<Participation>(p =>
            p.MatchId == 1 && p.MembreId == Joueur.Id && p.Role == RoleParticipation.Joueur && p.Statut == StatutParticipation.Reservee)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(Joueur.Id, result.MembreId);
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_MatchIsPublic_ThrowsInscriptionMatchPriveInterditeException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch());

        await Assert.ThrowsAsync<InscriptionMatchPriveInterditeException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 2, new AjouterJoueurDto("G2")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_MatchAlreadyPlayed_ThrowsMatchNonModifiableException()
    {
        var match = PrivateMatch();
        match.Date = new DateOnly(2026, 8, 1);
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);

        await Assert.ThrowsAsync<MatchNonModifiableException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("G2")));
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_CallerNotOrganizer_ThrowsMatchNotFoundException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G3")).ReturnsAsync(new Membre { Id = 3, Matricule = "G3", Name = "N", FirstName = "F", TypeMembreId = TypeMembreSeed.GlobalId });

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.AjouterJoueurMatchPriveAsync("G3", 1, new AjouterJoueurDto("G2")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_MatchAlreadyHasFourActive_ThrowsMatchCompletException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch(nbActifs: 4));

        await Assert.ThrowsAsync<MatchCompletException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("G2")));
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_UnknownJoueurMatricule_ThrowsMembreNotFoundByMatriculeException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundByMatriculeException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("G999")));
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_JoueurOnOtherSite_ThrowsSiteNonAutoriseException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        var joueurSite = new Membre
        {
            Id = 4, Matricule = "S1", Name = "N", FirstName = "F", TypeMembreId = TypeMembreSeed.SiteId, SiteId = 999,
            TypeMembre = new TypeMembre { Id = TypeMembreSeed.SiteId, Code = TypeMembreSeed.SiteCode, Libelle = "Site", PrefixeMatricule = "S", DelaiReservationJours = 14 }
        };
        _membreRepository.Setup(r => r.GetByMatriculeAsync("S1")).ReturnsAsync(joueurSite);

        await Assert.ThrowsAsync<SiteNonAutoriseException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("S1")));
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_JoueurHasOverlappingParticipation_ThrowsChevauchementMatchException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        _participationRepository.Setup(r => r.GetActiveByMembreIdAsync(Joueur.Id)).ReturnsAsync(
        [
            new Participation
            {
                MatchId = 777, NumeroPlace = 1, Statut = StatutParticipation.Reservee,
                Match = new Match { Id = 777, TerrainId = 6, OrganisateurId = 1, Date = new DateOnly(2026, 9, 10), StartTime = new TimeOnly(10, 30), EndTime = new TimeOnly(12, 0), TypeMatch = TypeMatch.Private, Statut = StatutMatch.Open }
            }
        ]);

        await Assert.ThrowsAsync<ChevauchementMatchException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("G2")));
    }

    [Fact]
    public async Task AjouterJoueurMatchPriveAsync_InvalidMatriculeFormat_ThrowsValidationException()
    {
        _ajouterJoueurValidator.Setup(v => v.ValidateAsync(It.IsAny<AjouterJoueurDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(AjouterJoueurDto.Matricule), "invalid")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.AjouterJoueurMatchPriveAsync("G1", 1, new AjouterJoueurDto("bad")));

        _matchRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    // --- RejoindreMatchPublicAsync (RG-PUB-002/003/004) ---

    [Fact]
    public async Task RejoindreMatchPublicAsync_HappyPath_AddsSelfAndSaves()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch());

        var result = await _sut.RejoindreMatchPublicAsync("G2", 2);

        _participationRepository.Verify(r => r.AddAsync(It.Is<Participation>(p =>
            p.MatchId == 2 && p.MembreId == Joueur.Id && p.Statut == StatutParticipation.Reservee)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(Joueur.Id, result.MembreId);
    }

    [Fact]
    public async Task RejoindreMatchPublicAsync_MatchIsPrivate_ThrowsRejoindreMatchPriveInterditException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());

        await Assert.ThrowsAsync<RejoindreMatchPriveInterditException>(() => _sut.RejoindreMatchPublicAsync("G2", 1));
    }

    [Fact]
    public async Task RejoindreMatchPublicAsync_MatchAlreadyPlayed_ThrowsMatchNonModifiableException()
    {
        var match = PublicMatch();
        match.Date = new DateOnly(2026, 8, 1);
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(match);

        await Assert.ThrowsAsync<MatchNonModifiableException>(() => _sut.RejoindreMatchPublicAsync("G2", 2));
    }

    [Fact]
    public async Task RejoindreMatchPublicAsync_MatchFull_ThrowsMatchCompletException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch(nbActifs: 4));

        await Assert.ThrowsAsync<MatchCompletException>(() => _sut.RejoindreMatchPublicAsync("G2", 2));
    }

    [Fact]
    public async Task RejoindreMatchPublicAsync_UnknownMatricule_ThrowsMembreNotFoundByMatriculeException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch());
        _membreRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Membre?)null);

        await Assert.ThrowsAsync<MembreNotFoundByMatriculeException>(() => _sut.RejoindreMatchPublicAsync("G999", 2));
    }

    // --- GetParticipantsAsync ---

    [Fact]
    public async Task GetParticipantsAsync_ExistingMatch_ReturnsParticipants()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch(nbActifs: 2));

        var result = await _sut.GetParticipantsAsync(2);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetParticipantsAsync_UnknownMatch_ThrowsMatchNotFoundException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Match?)null);

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.GetParticipantsAsync(999));
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
