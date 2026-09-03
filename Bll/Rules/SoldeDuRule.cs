using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-RES-006 / RG-PAY-006: an outstanding balance blocks new reservations.</summary>
public static class SoldeDuRule
{
    public static bool ADuSoldeImpaye(IEnumerable<SoldeDu> soldes) =>
        soldes.Any(s => s.Statut == StatutSoldeDu.Du);
}
