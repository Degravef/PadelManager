using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Bll.Rules;

/// RG-RES-006 / RG-PAY-006
public static class BalanceDueRule
{
    public static bool HasUnpaidBalance(IEnumerable<BalanceDue> balancesDue) =>
        balancesDue.Any(s => s.Status == BalanceDueStatus.Due);
}
