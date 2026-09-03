using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>RG-RES-007 / RG-PEN-002: an active penalty (still within its end date) blocks new reservations.</summary>
public static class PenaliteActiveRule
{
    public static bool EstActive(IEnumerable<Penalite> penalites, DateOnly aujourdHui) =>
        penalites.Any(p => p.Active && p.DateFin >= aujourdHui);
}
