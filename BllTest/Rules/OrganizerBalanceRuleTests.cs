using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class OrganizerBalanceRuleTests
{
    private static Participation P(ParticipationStatus status, decimal amount = 15m) => new() { MatchId = 1, SeatNumber = 1, Status = status, AmountDue = amount };

    [Fact]
    public void CalculateBalance_NoOnePaid_ReturnsFullAmount()
    {
        Assert.Equal(60m, OrganizerBalanceRule.CalculateBalance(60m, []));
    }

    [Fact]
    public void CalculateBalance_TwoPaidTwoUnpaid_ReturnsHalfAmount()
    {
        Participation[] participations = [P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Reserved), P(ParticipationStatus.Reserved)];

        Assert.Equal(30m, OrganizerBalanceRule.CalculateBalance(60m, participations));
    }

    [Fact]
    public void CalculateBalance_AllFourPaid_ReturnsZero()
    {
        Participation[] participations = [P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid)];

        Assert.Equal(0m, OrganizerBalanceRule.CalculateBalance(60m, participations));
    }
}
