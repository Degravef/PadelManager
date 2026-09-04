using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// <summary>RG-RES-006 / RG-PAY-006: an outstanding balance blocks new reservations.</summary>
public static class BalanceDueRule
{
    public static bool HasUnpaidBalance(IEnumerable<BalanceDue> balancesDue) =>
        balancesDue.Any(s => s.Status == BalanceDueStatus.Due);
}
