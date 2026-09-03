using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;


public static class SoldeDuRule
{
    public static bool ADuSoldeImpaye(IEnumerable<SoldeDu> soldes) =>
        soldes.Any(s => s.Statut == StatutSoldeDu.Du);
}
