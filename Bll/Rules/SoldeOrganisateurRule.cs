using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-PAY-005 / RG-PUB-006 / CF-RC-003: the organizer owes the price of every unsold/unpaid seat.</summary>
public static class SoldeOrganisateurRule
{
    public static decimal CalculerSolde(decimal montantTotal, IEnumerable<Participation> participations)
    {
        decimal montantPaye = participations.Where(p => p.Statut == StatutParticipation.Payee).Sum(p => p.MontantDu);
        return Math.Max(0m, montantTotal - montantPaye);
    }
}
