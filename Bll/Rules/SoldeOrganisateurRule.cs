using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;


public static class SoldeOrganisateurRule
{
    public static decimal CalculerSolde(decimal montantTotal, IEnumerable<Participation> participations)
    {
        decimal montantPaye = participations.Where(p => p.Statut == StatutParticipation.Payee).Sum(p => p.MontantDu);
        return Math.Max(0m, montantTotal - montantPaye);
    }
}
