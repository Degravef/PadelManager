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
using MatchType = Core.Domain.Enums.MatchType;

namespace BllTest.Services;

public class ParticipationServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<ICourtRepository> _courtRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<AddPlayerDto>> _addPlayerValidator = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly ParticipationService _sut;

    private static readonly MemberType TypeGlobal = new()
    {
        Id = MemberTypeSeed.GlobalId, Code = MemberTypeSeed.GlobalCode, Label = "Membre global", MatriculePrefix = "G", ReservationWindowDays = 21
    };
    private static readonly Member Organizer = new() { Id = 1, Matricule = "G1", Name = "N", FirstName = "F", MemberTypeId = MemberTypeSeed.GlobalId, MemberType = TypeGlobal };
    private static readonly Member Player = new() { Id = 2, Matricule = "G2", Name = "N2", FirstName = "F2", MemberTypeId = MemberTypeSeed.GlobalId, MemberType = TypeGlobal };
    private static readonly Court SomeCourt = new() { Id = 5, Name = "Court 1", SiteId = 1 };

    public ParticipationServiceTests()
    {
        _addPlayerValidator.Setup(v => v.ValidateAsync(It.IsAny<AddPlayerDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Organizer);
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G2")).ReturnsAsync(Player);
        _courtRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(SomeCourt);
        _participationRepository.Setup(r => r.GetActiveByMemberIdAsync(It.IsAny<int>())).ReturnsAsync([]);

        _sut = new ParticipationService(
            _matchRepository.Object, _participationRepository.Object, _courtRepository.Object, _memberRepository.Object,
            _unitOfWork.Object, _timeProvider, _addPlayerValidator.Object);
    }

    private static Match PrivateMatch(int activeCount = 1, MatchStatus status = MatchStatus.Open) => new()
    {
        Id = 1, CourtId = 5, OrganizerId = Organizer.Id, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Type = MatchType.Private, Status = status,
        TotalAmount = 60m,
        Participations = Enumerable.Range(1, activeCount)
            .Select(n => new Participation { MatchId = 1, MemberId = 100 + n, SeatNumber = n, Status = ParticipationStatus.Reserved })
            .ToList()
    };

    private static Match PublicMatch(int activeCount = 1, MatchStatus status = MatchStatus.Open) => new()
    {
        Id = 2, CourtId = 5, OrganizerId = Organizer.Id, Date = new DateOnly(2026, 9, 10),
        StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Type = MatchType.Public, Status = status,
        TotalAmount = 60m,
        Participations = Enumerable.Range(1, activeCount)
            .Select(n => new Participation { MatchId = 2, MemberId = 100 + n, SeatNumber = n, Status = ParticipationStatus.Reserved })
            .ToList()
    };

    // --- AddPlayerToPrivateMatchAsync (RG-PRV-001/002) ---

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_HappyPath_AddsPlayerAndSaves()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());

        var result = await _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("G2"));

        _participationRepository.Verify(r => r.AddAsync(It.Is<Participation>(p =>
            p.MatchId == 1 && p.MemberId == Player.Id && p.Role == ParticipationRole.Player && p.Status == ParticipationStatus.Reserved)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(Player.Id, result.MemberId);
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_MatchIsPublic_ThrowsPrivateMatchRegistrationForbiddenException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch());

        await Assert.ThrowsAsync<PrivateMatchRegistrationForbiddenException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 2, new AddPlayerDto("G2")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_MatchAlreadyPlayed_ThrowsMatchNotModifiableException()
    {
        var match = PrivateMatch();
        match.Date = new DateOnly(2026, 8, 1);
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);

        await Assert.ThrowsAsync<MatchNotModifiableException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("G2")));
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_CallerNotOrganizer_ThrowsMatchNotFoundException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G3")).ReturnsAsync(new Member { Id = 3, Matricule = "G3", Name = "N", FirstName = "F", MemberTypeId = MemberTypeSeed.GlobalId });

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.AddPlayerToPrivateMatchAsync("G3", 1, new AddPlayerDto("G2")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_MatchAlreadyHasFourActive_ThrowsMatchFullException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch(activeCount: 4));

        await Assert.ThrowsAsync<MatchFullException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("G2")));
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_UnknownPlayerMatricule_ThrowsMemberNotFoundByMatriculeException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundByMatriculeException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("G999")));
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_PlayerOnOtherSite_ThrowsSiteNotAuthorizedException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        var playerOnSite = new Member
        {
            Id = 4, Matricule = "S1", Name = "N", FirstName = "F", MemberTypeId = MemberTypeSeed.SiteId, SiteId = 999,
            MemberType = new MemberType { Id = MemberTypeSeed.SiteId, Code = MemberTypeSeed.SiteCode, Label = "Site", MatriculePrefix = "S", ReservationWindowDays = 14 }
        };
        _memberRepository.Setup(r => r.GetByMatriculeAsync("S1")).ReturnsAsync(playerOnSite);

        await Assert.ThrowsAsync<SiteNotAuthorizedException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("S1")));
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_PlayerHasOverlappingParticipation_ThrowsMatchOverlapException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());
        _participationRepository.Setup(r => r.GetActiveByMemberIdAsync(Player.Id)).ReturnsAsync(
        [
            new Participation
            {
                MatchId = 777, SeatNumber = 1, Status = ParticipationStatus.Reserved,
                Match = new Match { Id = 777, CourtId = 6, OrganizerId = 1, Date = new DateOnly(2026, 9, 10), StartTime = new TimeOnly(10, 30), EndTime = new TimeOnly(12, 0), Type = MatchType.Private, Status = MatchStatus.Open }
            }
        ]);

        await Assert.ThrowsAsync<MatchOverlapException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("G2")));
    }

    [Fact]
    public async Task AddPlayerToPrivateMatchAsync_InvalidMatriculeFormat_ThrowsValidationException()
    {
        _addPlayerValidator.Setup(v => v.ValidateAsync(It.IsAny<AddPlayerDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(AddPlayerDto.Matricule), "invalid")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.AddPlayerToPrivateMatchAsync("G1", 1, new AddPlayerDto("bad")));

        _matchRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    // --- JoinPublicMatchAsync (RG-PUB-002/003/004) ---

    [Fact]
    public async Task JoinPublicMatchAsync_HappyPath_AddsSelfAndSaves()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch());

        var result = await _sut.JoinPublicMatchAsync("G2", 2);

        _participationRepository.Verify(r => r.AddAsync(It.Is<Participation>(p =>
            p.MatchId == 2 && p.MemberId == Player.Id && p.Status == ParticipationStatus.Reserved)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(Player.Id, result.MemberId);
    }

    [Fact]
    public async Task JoinPublicMatchAsync_MatchIsPrivate_ThrowsJoinPrivateMatchForbiddenException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PrivateMatch());

        await Assert.ThrowsAsync<JoinPrivateMatchForbiddenException>(() => _sut.JoinPublicMatchAsync("G2", 1));
    }

    [Fact]
    public async Task JoinPublicMatchAsync_MatchAlreadyPlayed_ThrowsMatchNotModifiableException()
    {
        var match = PublicMatch();
        match.Date = new DateOnly(2026, 8, 1);
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(match);

        await Assert.ThrowsAsync<MatchNotModifiableException>(() => _sut.JoinPublicMatchAsync("G2", 2));
    }

    [Fact]
    public async Task JoinPublicMatchAsync_MatchFull_ThrowsMatchFullException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch(activeCount: 4));

        await Assert.ThrowsAsync<MatchFullException>(() => _sut.JoinPublicMatchAsync("G2", 2));
    }

    [Fact]
    public async Task JoinPublicMatchAsync_UnknownMatricule_ThrowsMemberNotFoundByMatriculeException()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch());
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundByMatriculeException>(() => _sut.JoinPublicMatchAsync("G999", 2));
    }

    // --- GetParticipantsAsync ---

    [Fact]
    public async Task GetParticipantsAsync_ExistingMatch_ReturnsParticipants()
    {
        _matchRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(PublicMatch(activeCount: 2));

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
