using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-PRV-001 / RG-ETA-001: participant headcount helpers for a match.</summary>
public static class RosterCompleteRule
{
    public static int CountActive(IEnumerable<Participation> participations) =>
        participations.Count(p => p.Status is ParticipationStatus.Reserved or ParticipationStatus.Paid);

    public static int CountPaid(IEnumerable<Participation> participations) =>
        participations.Count(p => p.Status == ParticipationStatus.Paid);

    public static bool HasFourActive(IEnumerable<Participation> participations) => CountActive(participations) >= 4;

    public static bool IsComplete(IEnumerable<Participation> participations) => CountPaid(participations) >= 4;
}
