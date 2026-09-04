using Bll.Extensions;
using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using FluentValidation;
using MatchType = Core.Domain.Enums.MatchType;

namespace Bll.Services;

public class ReservationService(
    IMatchRepository matchRepository,
    IParticipationRepository participationRepository,
    ICourtRepository courtRepository,
    IMemberRepository memberRepository,
    ISiteScheduleRepository siteScheduleRepository,
    IClosureDayRepository closureDayRepository,
    IBalanceDueRepository balanceDueRepository,
    IPenaltyRepository penaltyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IValidator<CreateReservationDto> createValidator) : IReservationService
{
    public async Task<MatchDto> GetReservationByIdAsync(string matricule, int id)
    {
        Match match = await GetMatchOrThrowAsync(id);

        // RG-PRV-003
        if (match.Type == MatchType.Private)
        {
            Member? caller = await memberRepository.GetByMatriculeAsync(matricule);
            bool isParticipant = caller is not null && match.Participations.Any(p => p.MemberId == caller.Id);
            if (!isParticipant)
                throw new MatchNotFoundException(id);
        }

        return ToDto(match);
    }

    public async Task<IEnumerable<MatchDto>> GetMyReservationsAsync(string matricule)
    {
        Member member = await GetMemberOrThrowAsync(matricule);
        IEnumerable<Match> matches = await matchRepository.GetByOrganizerIdAsync(member.Id);
        return matches.Select(ToDto);
    }

    public async Task<MatchDto> CreateReservationAsync(string matricule, CreateReservationDto dto)
    {
        await createValidator.ValidateOrThrowAsync(dto);

        Member organizer = await GetMemberOrThrowAsync(matricule);
        Court court = await GetCourtOrThrowAsync(dto.CourtId);

        // RG-RES-002
        if (!court.Active)
            throw new CourtInactiveException();

        // RG-MEM-005/006/007
        if (!MemberScopeRule.CanActOnSite(organizer, court.SiteId))
            throw new SiteNotAuthorizedException();

        // RG-RES-001
        DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (!ReservationWindowRule.IsWithinWindow(organizer.MemberType!.ReservationWindowDays, dto.Date, today))
            throw new ReservationWindowExceededException();

        // RG-RES-006 / RG-PAY-006
        IEnumerable<BalanceDue> unpaidBalances = await balanceDueRepository.GetOutstandingByMemberIdAsync(organizer.Id);
        if (BalanceDueRule.HasUnpaidBalance(unpaidBalances))
            throw new BalanceDueException();

        // RG-RES-007 / RG-PEN-002
        IEnumerable<Penalty> activePenalties = await penaltyRepository.GetActiveByMemberIdAsync(organizer.Id);
        if (ActivePenaltyRule.IsActive(activePenalties, today))
            throw new ActivePenaltyException();

        // RG-SITE-002/003/004/005
        SiteSchedule? schedule = await siteScheduleRepository.GetBySiteAndYearAsync(court.SiteId, dto.Date.Year);
        if (schedule is null)
            throw new SiteScheduleNotDefinedException(court.SiteId, dto.Date.Year);
        if (!AvailableSlotsRule.Calculate(schedule).Contains(dto.StartTime))
            throw new SlotOutsideOpeningHoursException();

        // RG-SITE-007/008
        IEnumerable<ClosureDay> siteClosures = await closureDayRepository.GetBySiteIdAsync(court.SiteId);
        IEnumerable<ClosureDay> globalClosures = await closureDayRepository.GetGlobalAsync();
        if (!SiteOpenRule.IsOpen(siteClosures.Concat(globalClosures), dto.Date))
            throw new ClosureDayException();

        // RG-SITE-006
        IEnumerable<Match> matchesSameDaySameCourt = await matchRepository.GetByCourtAndDateAsync(dto.CourtId, dto.Date);
        if (!SlotAvailableRule.IsAvailable(matchesSameDaySameCourt, dto.StartTime))
            throw new SlotUnavailableException();

        // RG-ETA-006
        TimeOnly endTime = dto.StartTime.Add(TimeSpan.FromMinutes(schedule.MatchDurationMinutes));
        IEnumerable<Participation> activeParticipations = await participationRepository.GetActiveByMemberIdAsync(organizer.Id);
        if (OverlapRule.IsOverlapping(activeParticipations, currentMatchId: 0, dto.Date, dto.StartTime, endTime))
            throw new MatchOverlapException();

        var match = new Match
        {
            CourtId = dto.CourtId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Type = dto.IsPublic ? MatchType.Public : MatchType.Private,
            Status = MatchStatus.Open,
            OrganizerId = organizer.Id,
            TotalAmount = schedule.MatchPrice,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            PaymentDeadline = dto.Date.AddDays(-1)
        };
        await matchRepository.AddAsync(match);

        var participation = new Participation
        {
            Match = match,
            MatchId = match.Id,
            MemberId = organizer.Id,
            SeatNumber = 1,
            Role = ParticipationRole.Organizer,
            Status = ParticipationStatus.Reserved,
            AmountDue = schedule.MatchPrice / 4m,
            RegistrationDate = timeProvider.GetUtcNow().UtcDateTime
        };
        await participationRepository.AddAsync(participation);

        await unitOfWork.SaveChangesAsync();

        return ToDto(match);
    }

    public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(int siteId, DateOnly date)
    {
        // RG-SITE-002/003/004/005
        SiteSchedule? schedule = await siteScheduleRepository.GetBySiteAndYearAsync(siteId, date.Year);
        if (schedule is null)
            return [];

        // RG-SITE-007/008
        IEnumerable<ClosureDay> siteClosures = await closureDayRepository.GetBySiteIdAsync(siteId);
        IEnumerable<ClosureDay> globalClosures = await closureDayRepository.GetGlobalAsync();
        if (!SiteOpenRule.IsOpen(siteClosures.Concat(globalClosures), date))
            return [];

        IReadOnlyList<TimeOnly> possibleStartTimes = AvailableSlotsRule.Calculate(schedule);
        TimeSpan matchDuration = TimeSpan.FromMinutes(schedule.MatchDurationMinutes);

        // RG-RES-002
        IEnumerable<Court> courts = (await courtRepository.GetBySiteIdAsync(siteId)).Where(t => t.Active);
        IEnumerable<Match> matchesOfDay = (await matchRepository.GetBySiteAndDateAsync(siteId, date)).ToList();

        var slots = new List<AvailableSlotDto>();
        foreach (Court court in courts)
        {
            IEnumerable<Match> matchesSameCourt = matchesOfDay.Where(m => m.CourtId == court.Id);
            foreach (TimeOnly startTime in possibleStartTimes)
            {
                // RG-SITE-006
                if (SlotAvailableRule.IsAvailable(matchesSameCourt, startTime))
                    slots.Add(new AvailableSlotDto(court.Id, court.Name, startTime, startTime.Add(matchDuration)));
            }
        }

        return slots.OrderBy(s => s.StartTime).ThenBy(s => s.CourtName);
    }

    private async Task<Match> GetMatchOrThrowAsync(int id)
    {
        Match? match = await matchRepository.GetByIdAsync(id);
        if (match is null)
            throw new MatchNotFoundException(id);
        return match;
    }

    private async Task<Member> GetMemberOrThrowAsync(string matricule)
    {
        Member? member = await memberRepository.GetByMatriculeAsync(matricule);
        if (member is null)
            throw new MemberNotFoundByMatriculeException(matricule);
        return member;
    }

    private async Task<Court> GetCourtOrThrowAsync(int courtId)
    {
        Court? court = await courtRepository.GetByIdAsync(courtId);
        if (court is null)
            throw new CourtNotFoundException(courtId);
        return court;
    }

    private static MatchDto ToDto(Match match) => new(
        match.Id, match.CourtId, match.Date, match.StartTime,
        match.Type.ToString(), match.Status.ToString(), match.OrganizerId, match.TotalAmount,
        match.EndTime, match.AmountPaid, match.CreatedAt, match.PaymentDeadline, match.PublicSwitchDate);
}
