using Core.Domain.Entities;

namespace Bll.Rules;


public static class PenaliteActiveRule
{
    public static bool EstActive(IEnumerable<Penalite> penalites, DateOnly aujourdHui) =>
        penalites.Any(p => p.Active && p.DateFin >= aujourdHui);
}
