using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class BalanceDueRuleTests
{
    [Fact]
    public void HasUnpaidBalance_NoBalances_ReturnsFalse()
    {
        Assert.False(BalanceDueRule.HasUnpaidBalance([]));
    }

    [Fact]
    public void HasUnpaidBalance_OnlyPaidBalances_ReturnsFalse()
    {
        BalanceDue[] balances = [new() { MemberId = 1, MatchId = 1, Status = BalanceDueStatus.Paid }];

        Assert.False(BalanceDueRule.HasUnpaidBalance(balances));
    }

    [Fact]
    public void HasUnpaidBalance_OneOutstandingBalance_ReturnsTrue()
    {
        BalanceDue[] balances = [new() { MemberId = 1, MatchId = 1, Status = BalanceDueStatus.Due }];

        Assert.True(BalanceDueRule.HasUnpaidBalance(balances));
    }
}
