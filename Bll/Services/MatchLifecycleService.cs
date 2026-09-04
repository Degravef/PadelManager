using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using MatchType = Core.Domain.Enums.MatchType;

namespace Bll.Services;

// RG-ETA-002/003: the J-1 daily batch — private matches with an incomplete roster or an unpaid player
// switch to Public (the former also earns the organizer a 1-week penalty), and every match that's Public
// by the end of this pass either gets an organizer balance due for its unsold seats, or is marked Complete.
public class MatchLifecycleService(
    IMatchRepository matchRepository,
    IParticipationRepository participationRepository,
    IPenaltyRepository penaltyRepository,
    IBalanceDueRepository balanceDueRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IMatchLifecycleService
{
    public async Task<DailyBatchResultDto> ExecuteDailyBatchAsync(DateOnly? today = null)
    {
        DateOnly processedDate = today ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        DateOnly tomorrow = processedDate.AddDays(1);
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        var matches = (await matchRepository.GetByDateAsync(tomorrow))
            .Where(m => m.Status != MatchStatus.Cancelled)
            .ToList();

        int switchedIncompleteRoster = 0, switchedUnpaidSeat = 0, penaltiesApplied = 0, balancesDueCreated = 0, matchesCompleted = 0;

        foreach (Match match in matches)
        {
            if (match.Type == MatchType.Private)
            {
                if (!RosterCompleteRule.HasFourActive(match.Participations))
                {
                    // RG-PRV-004/005: incomplete roster -> switch to public + organizer penalty.
                    SwitchToPublic(match, now);
                    switchedIncompleteRoster++;

                    await penaltyRepository.AddAsync(new Penalty
                    {
                        MemberId = match.OrganizerId,
                        MatchId = match.Id,
                        Reason = "Effectif incomplet la veille du match (RG-PRV-004/005).",
                        StartDate = processedDate,
                        EndDate = processedDate.AddDays(7),
                        Active = true
                    });
                    penaltiesApplied++;
                }
                else
                {
                    // RG-PAY-004: 4 players registered but one unpaid -> their seat frees up, switch to public.
                    var unpaid = match.Participations.Where(p => p.Status == ParticipationStatus.Reserved).ToList();
                    if (unpaid.Count > 0)
                    {
                        foreach (Participation participation in unpaid)
                        {
                            participationRepository.Delete(participation);
                            match.Participations.Remove(participation);
                        }

                        SwitchToPublic(match, now);
                        switchedUnpaidSeat++;
                    }
                }
            }

            if (match.Type == MatchType.Public)
            {
                if (RosterCompleteRule.IsComplete(match.Participations))
                {
                    match.Status = MatchStatus.Complete;
                    matchRepository.Update(match);
                    matchesCompleted++;
                }
                else
                {
                    // RG-PUB-006 / RG-PAY-005: balance due for unsold seats (idempotent across runs).
                    BalanceDue? existing = await balanceDueRepository.GetByMatchIdAsync(match.Id);
                    if (existing is null)
                    {
                        decimal amount = OrganizerBalanceRule.CalculateBalance(match.TotalAmount, match.Participations);
                        if (amount > 0)
                        {
                            await balanceDueRepository.AddAsync(new BalanceDue
                            {
                                MemberId = match.OrganizerId,
                                MatchId = match.Id,
                                Amount = amount,
                                Status = BalanceDueStatus.Due,
                                CreatedAt = now
                            });
                            balancesDueCreated++;
                        }
                    }
                }
            }
        }

        await unitOfWork.SaveChangesAsync();

        return new DailyBatchResultDto(tomorrow, switchedIncompleteRoster, switchedUnpaidSeat, penaltiesApplied, balancesDueCreated, matchesCompleted);
    }

    private void SwitchToPublic(Match match, DateTime now)
    {
        match.Type = MatchType.Public; // RG-ETA-004: never switches back to private.
        match.PublicSwitchDate = now;
        matchRepository.Update(match);
    }
}
