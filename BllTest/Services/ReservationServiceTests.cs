using Bll.Rules;
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

public class ReservationServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IParticipationRepository> _participationRepository = new();
    private readonly Mock<ICourtRepository> _courtRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();
    private readonly Mock<ISiteScheduleRepository> _siteScheduleRepository = new();
    private readonly Mock<IClosureDayRepository> _closureDayRepository = new();
    private readonly Mock<IBalanceDueRepository> _balanceDueRepository = new();
    private readonly Mock<IPenaltyRepository> _penaltyRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateReservationDto>> _createValidator = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly ReservationService _sut;

    private static readonly MemberType TypeGlobal = new()
    {
        Id = MemberTypeSeed.GlobalId, Code = MemberTypeSeed.GlobalCode, Label = "Membre global", MatriculePrefix = "G", ReservationWindowDays = 21
    };
    private static readonly MemberType TypeSite = new()
    {
        Id = MemberTypeSeed.SiteId, Code = MemberTypeSeed.SiteCode, Label = "Membre de site", MatriculePrefix = "S", ReservationWindowDays = 14
    };
    private static readonly Member Organizer = new()
    {
        Id = 10, Matricule = "G1", Name = "Doe", FirstName = "Jane", MemberTypeId = MemberTypeSeed.GlobalId, MemberType = TypeGlobal
    };
    private static readonly Court SomeCourt = new() { Id = 5, Name = "Court 1", SiteId = 1, Active = true };
    private static readonly SiteSchedule SomeSchedule = new()
    {
        Id = 1, SiteId = 1, Year = 2026, OpeningTime = new TimeOnly(10, 0), ClosingTime = new TimeOnly(20, 0),
        MatchDurationMinutes = 90, BreakMinutes = 15, MatchPrice = 60m
    };
    private static readonly CreateReservationDto ValidDto = new(5, new DateOnly(2026, 9, 5), new TimeOnly(10, 0));

    public ReservationServiceTests()
    {
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateReservationDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G1")).ReturnsAsync(Organizer);
        _courtRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(SomeCourt);
        _siteScheduleRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync(SomeSchedule);
        _closureDayRepository.Setup(r => r.GetBySiteIdAsync(1)).ReturnsAsync([]);
        _closureDayRepository.Setup(r => r.GetGlobalAsync()).ReturnsAsync([]);
        _balanceDueRepository.Setup(r => r.GetOutstandingByMemberIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _penaltyRepository.Setup(r => r.GetActiveByMemberIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _participationRepository.Setup(r => r.GetActiveByMemberIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        _matchRepository.Setup(r => r.GetByCourtAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>())).ReturnsAsync([]);

        _sut = new ReservationService(
            _matchRepository.Object, _participationRepository.Object, _courtRepository.Object, _memberRepository.Object,
            _siteScheduleRepository.Object, _closureDayRepository.Object, _balanceDueRepository.Object, _penaltyRepository.Object,
            _unitOfWork.Object, _timeProvider, _createValidator.Object);
    }

    [Fact]
    public async Task CreateReservationAsync_HappyPath_CreatesPrivateMatchWithOrganizerAsParticipant_AndSavesOnce()
    {
        var result = await _sut.CreateReservationAsync("G1", ValidDto);

        _matchRepository.Verify(r => r.AddAsync(It.Is<Match>(m =>
            m.CourtId == 5 && m.Date == ValidDto.Date && m.StartTime == ValidDto.StartTime &&
            m.Type == MatchType.Private && m.Status == MatchStatus.Open &&
            m.OrganizerId == Organizer.Id && m.TotalAmount == 60m)), Times.Once);

        _participationRepository.Verify(r => r.AddAsync(It.Is<Participation>(p =>
            p.MemberId == Organizer.Id && p.AmountDue == 15m &&
            p.SeatNumber == 1 && p.Role == ParticipationRole.Organizer && p.Status == ParticipationStatus.Reserved)), Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal("Private", result.Type);
        Assert.Equal("Open", result.Status);
        Assert.Equal(Organizer.Id, result.OrganizerId);
        Assert.Equal(60m, result.TotalAmount);
    }

    [Fact]
    public async Task CreateReservationAsync_IsPublicTrue_CreatesPublicMatch()
    {
        var dto = ValidDto with { IsPublic = true };

        var result = await _sut.CreateReservationAsync("G1", dto);

        Assert.Equal("Public", result.Type);
    }

    [Fact]
    public async Task CreateReservationAsync_UnknownMatricule_ThrowsMemberNotFoundByMatriculeException_AndNeverSaves()
    {
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundByMatriculeException>(() => _sut.CreateReservationAsync("G999", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_UnknownCourt_ThrowsCourtNotFoundException_AndNeverSaves()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Court?)null);
        var dto = ValidDto with { CourtId = 999 };

        await Assert.ThrowsAsync<CourtNotFoundException>(() => _sut.CreateReservationAsync("G1", dto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_CourtInactive_ThrowsCourtInactiveException_AndNeverSaves()
    {
        _courtRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Court { Id = 5, Name = "Court 1", SiteId = 1, Active = false });

        await Assert.ThrowsAsync<CourtInactiveException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_SiteMemberOnOtherSite_ThrowsSiteNotAuthorizedException_AndNeverSaves()
    {
        var siteMember = new Member { Id = 20, Matricule = "S1", Name = "N", FirstName = "F", MemberTypeId = MemberTypeSeed.SiteId, MemberType = TypeSite, SiteId = 999 };
        _memberRepository.Setup(r => r.GetByMatriculeAsync("S1")).ReturnsAsync(siteMember);

        await Assert.ThrowsAsync<SiteNotAuthorizedException>(() => _sut.CreateReservationAsync("S1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_SiteMemberOnOwnSite_Succeeds()
    {
        var siteMember = new Member { Id = 20, Matricule = "S1", Name = "N", FirstName = "F", MemberTypeId = MemberTypeSeed.SiteId, MemberType = TypeSite, SiteId = 1 };
        _memberRepository.Setup(r => r.GetByMatriculeAsync("S1")).ReturnsAsync(siteMember);

        var result = await _sut.CreateReservationAsync("S1", ValidDto);

        Assert.Equal(20, result.OrganizerId);
    }

    [Fact]
    public async Task CreateReservationAsync_OutsideBookingWindow_ThrowsReservationWindowExceededException_AndNeverSaves()
    {
        // Global member: 21-day window. Match is 30 days out from "today" (2026-09-01) — too early.
        var dto = ValidDto with { Date = new DateOnly(2026, 10, 1) };

        await Assert.ThrowsAsync<ReservationWindowExceededException>(() => _sut.CreateReservationAsync("G1", dto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_UnpaidBalanceDue_ThrowsBalanceDueException_AndNeverSaves()
    {
        _balanceDueRepository.Setup(r => r.GetOutstandingByMemberIdAsync(Organizer.Id))
            .ReturnsAsync([new BalanceDue { MemberId = Organizer.Id, MatchId = 1, Status = BalanceDueStatus.Due, Amount = 15m }]);

        await Assert.ThrowsAsync<BalanceDueException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ActivePenalty_ThrowsActivePenaltyException_AndNeverSaves()
    {
        _penaltyRepository.Setup(r => r.GetActiveByMemberIdAsync(Organizer.Id))
            .ReturnsAsync([new Penalty { MemberId = Organizer.Id, StartDate = new DateOnly(2026, 8, 30), EndDate = new DateOnly(2026, 9, 6), Active = true }]);

        await Assert.ThrowsAsync<ActivePenaltyException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_NoScheduleDefined_ThrowsSiteScheduleNotDefinedException_AndNeverSaves()
    {
        _siteScheduleRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync((SiteSchedule?)null);

        await Assert.ThrowsAsync<SiteScheduleNotDefinedException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_StartTimeNotAGeneratedSlot_ThrowsSlotOutsideOpeningHoursException_AndNeverSaves()
    {
        var dto = ValidDto with { StartTime = new TimeOnly(10, 5) };

        await Assert.ThrowsAsync<SlotOutsideOpeningHoursException>(() => _sut.CreateReservationAsync("G1", dto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_ClosureDay_ThrowsClosureDayException_AndNeverSaves()
    {
        _closureDayRepository.Setup(r => r.GetBySiteIdAsync(1))
            .ReturnsAsync([new ClosureDay { SiteId = 1, ClosureDate = ValidDto.Date }]);

        await Assert.ThrowsAsync<ClosureDayException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_GlobalClosureDay_ThrowsClosureDayException_AndNeverSaves()
    {
        _closureDayRepository.Setup(r => r.GetGlobalAsync())
            .ReturnsAsync([new ClosureDay { SiteId = null, ClosureDate = ValidDto.Date }]);

        await Assert.ThrowsAsync<ClosureDayException>(() => _sut.CreateReservationAsync("G1", ValidDto));
    }

    [Fact]
    public async Task CreateReservationAsync_ExactDoubleBooking_ThrowsSlotUnavailableException_AndNeverSaves()
    {
        _matchRepository.Setup(r => r.GetByCourtAndDateAsync(5, ValidDto.Date)).ReturnsAsync(
        [
            new Match { CourtId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30), Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = 99 }
        ]);

        await Assert.ThrowsAsync<SlotUnavailableException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _matchRepository.Verify(r => r.AddAsync(It.IsAny<Match>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_DifferentStartTimeSameCourtAndDate_Succeeds()
    {
        _matchRepository.Setup(r => r.GetByCourtAndDateAsync(5, ValidDto.Date)).ReturnsAsync(
        [
            new Match { CourtId = 5, Date = ValidDto.Date, StartTime = new TimeOnly(11, 45), EndTime = new TimeOnly(13, 15), Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = 99 }
        ]);

        var result = await _sut.CreateReservationAsync("G1", ValidDto);

        Assert.Equal(ValidDto.StartTime, result.StartTime);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_OverlappingParticipationOnAnotherMatch_ThrowsMatchOverlapException_AndNeverSaves()
    {
        _participationRepository.Setup(r => r.GetActiveByMemberIdAsync(Organizer.Id)).ReturnsAsync(
        [
            new Participation
            {
                MatchId = 777, SeatNumber = 1, Status = ParticipationStatus.Reserved,
                Match = new Match { Id = 777, CourtId = 6, OrganizerId = 1, Date = ValidDto.Date, StartTime = new TimeOnly(10, 30), EndTime = new TimeOnly(12, 0), Type = MatchType.Private, Status = MatchStatus.Open }
            }
        ]);

        await Assert.ThrowsAsync<MatchOverlapException>(() => _sut.CreateReservationAsync("G1", ValidDto));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_InvalidDto_ThrowsValidationException_AndNeverSaves()
    {
        var dto = ValidDto with { CourtId = 0 };
        _createValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure(nameof(CreateReservationDto.CourtId), "'Court Id' must be greater than '0'.")]));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateReservationAsync("G1", dto));

        _matchRepository.Verify(r => r.AddAsync(It.IsAny<Match>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetReservationByIdAsync_ViewedByOrganizer_ReturnsDto()
    {
        var match = new Match
        {
            Id = 1, CourtId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30),
            Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = Organizer.Id,
            Participations = [new Participation { MatchId = 1, MemberId = Organizer.Id, SeatNumber = 1, Status = ParticipationStatus.Reserved }]
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
            Id = 1, CourtId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30),
            Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = Organizer.Id,
            Participations = [new Participation { MatchId = 1, MemberId = Organizer.Id, SeatNumber = 1, Status = ParticipationStatus.Reserved }]
        };
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _memberRepository.Setup(r => r.GetByMatriculeAsync("L1")).ReturnsAsync(new Member { Id = 30, Matricule = "L1", Name = "N", FirstName = "F", MemberTypeId = 3 });

        await Assert.ThrowsAsync<MatchNotFoundException>(() => _sut.GetReservationByIdAsync("L1", 1));
    }

    [Fact]
    public async Task GetReservationByIdAsync_PublicMatchViewedByAnyone_ReturnsDto()
    {
        var match = new Match
        {
            Id = 1, CourtId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, EndTime = new TimeOnly(11, 30),
            Type = MatchType.Public, Status = MatchStatus.Open, OrganizerId = Organizer.Id
        };
        _matchRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(match);
        _memberRepository.Setup(r => r.GetByMatriculeAsync("L1")).ReturnsAsync((Member?)null);

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
    public async Task GetMyReservationsAsync_ExistingMember_ReturnsOwnMatchesAsDtos()
    {
        _matchRepository.Setup(r => r.GetByOrganizerIdAsync(Organizer.Id)).ReturnsAsync(
        [
            new Match { Id = 1, CourtId = 5, Date = ValidDto.Date, StartTime = ValidDto.StartTime, Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = Organizer.Id }
        ]);

        var result = await _sut.GetMyReservationsAsync("G1");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetMyReservationsAsync_UnknownMatricule_ThrowsMemberNotFoundByMatriculeException()
    {
        _memberRepository.Setup(r => r.GetByMatriculeAsync("G999")).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<MemberNotFoundByMatriculeException>(() => _sut.GetMyReservationsAsync("G999"));
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_NoScheduleDefined_ReturnsEmpty()
    {
        _siteScheduleRepository.Setup(r => r.GetBySiteAndYearAsync(1, 2026)).ReturnsAsync((SiteSchedule?)null);

        var result = await _sut.GetAvailableSlotsAsync(1, ValidDto.Date);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ClosureDay_ReturnsEmpty()
    {
        _closureDayRepository.Setup(r => r.GetBySiteIdAsync(1))
            .ReturnsAsync([new ClosureDay { SiteId = 1, ClosureDate = ValidDto.Date }]);

        var result = await _sut.GetAvailableSlotsAsync(1, ValidDto.Date);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_NoBookings_ReturnsEveryGeneratedSlotForEveryActiveCourt()
    {
        _courtRepository.Setup(r => r.GetBySiteIdAsync(1)).ReturnsAsync(
        [
            SomeCourt,
            new Court { Id = 6, Name = "Court 2", SiteId = 1, Active = true },
            new Court { Id = 7, Name = "Court 3 (inactif)", SiteId = 1, Active = false }
        ]);
        _matchRepository.Setup(r => r.GetBySiteAndDateAsync(1, ValidDto.Date)).ReturnsAsync([]);

        IReadOnlyList<TimeOnly> slots = AvailableSlotsRule.Calculate(SomeSchedule);

        var result = (await _sut.GetAvailableSlotsAsync(1, ValidDto.Date)).ToList();

        Assert.Equal(slots.Count * 2, result.Count);
        Assert.DoesNotContain(result, s => s.CourtId == 7);
        Assert.Contains(result, s => s.CourtId == 5 && s.StartTime == slots[0] && s.EndTime == slots[0].Add(TimeSpan.FromMinutes(90)));
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_CourtAlreadyBooked_ExcludesOnlyThatCourtAndStartTime()
    {
        _courtRepository.Setup(r => r.GetBySiteIdAsync(1)).ReturnsAsync(
        [
            SomeCourt,
            new Court { Id = 6, Name = "Court 2", SiteId = 1, Active = true }
        ]);
        _matchRepository.Setup(r => r.GetBySiteAndDateAsync(1, ValidDto.Date)).ReturnsAsync(
        [
            new Match { CourtId = 5, Date = ValidDto.Date, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Type = MatchType.Private, Status = MatchStatus.Open, OrganizerId = 99 }
        ]);

        var result = (await _sut.GetAvailableSlotsAsync(1, ValidDto.Date)).ToList();

        Assert.DoesNotContain(result, s => s.CourtId == 5 && s.StartTime == new TimeOnly(10, 0));
        Assert.Contains(result, s => s.CourtId == 6 && s.StartTime == new TimeOnly(10, 0));
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_CancelledMatch_SlotStaysAvailable()
    {
        _courtRepository.Setup(r => r.GetBySiteIdAsync(1)).ReturnsAsync([SomeCourt]);
        _matchRepository.Setup(r => r.GetBySiteAndDateAsync(1, ValidDto.Date)).ReturnsAsync(
        [
            new Match { CourtId = 5, Date = ValidDto.Date, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Type = MatchType.Private, Status = MatchStatus.Cancelled, OrganizerId = 99 }
        ]);

        var result = await _sut.GetAvailableSlotsAsync(1, ValidDto.Date);

        Assert.Contains(result, s => s.CourtId == 5 && s.StartTime == new TimeOnly(10, 0));
    }

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
