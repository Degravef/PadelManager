using Core.Domain.Entities;

namespace Bll.Rules;

/// RG-SITE-007/008
public static class SiteOpenRule
{
    public static bool IsOpen(IEnumerable<ClosureDay> closureDays, DateOnly date) =>
        closureDays.All(f => f.ClosureDate != date);
}
