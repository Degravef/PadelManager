using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>RG-RES-007 / RG-PEN-002: an active penalty (still within its end date) blocks new reservations.</summary>
public static class ActivePenaltyRule
{
    public static bool IsActive(IEnumerable<Penalty> penalties, DateOnly today) =>
        penalties.Any(p => p.Active && p.EndDate >= today);
}
