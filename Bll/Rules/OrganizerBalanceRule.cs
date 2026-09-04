using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-PAY-005 / RG-PUB-006 / CF-RC-003: the organizer owes the price of every unsold/unpaid seat.</summary>
public static class OrganizerBalanceRule
{
    public static decimal CalculateBalance(decimal totalAmount, IEnumerable<Participation> participations)
    {
        decimal amountPaid = participations.Where(p => p.Status == ParticipationStatus.Paid).Sum(p => p.AmountDue);
        return Math.Max(0m, totalAmount - amountPaid);
    }
}
