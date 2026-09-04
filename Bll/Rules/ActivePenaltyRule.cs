using Core.Domain.Entities;

namespace Bll.Rules;

/// RG-RES-007 / RG-PEN-002
public static class ActivePenaltyRule
{
    public static bool IsActive(IEnumerable<Penalty> penalties, DateOnly today) =>
        penalties.Any(p => p.Active && p.EndDate >= today);
}
