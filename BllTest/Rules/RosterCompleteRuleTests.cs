using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class RosterCompleteRuleTests
{
    private static Participation P(ParticipationStatus status) => new() { MatchId = 1, SeatNumber = 1, Status = status };

    [Fact]
    public void HasFourActive_ThreeReservedOnePaid_ReturnsTrue()
    {
        Participation[] participations = [P(ParticipationStatus.Reserved), P(ParticipationStatus.Reserved), P(ParticipationStatus.Reserved), P(ParticipationStatus.Paid)];

        Assert.True(RosterCompleteRule.HasFourActive(participations));
    }

    [Fact]
    public void HasFourActive_OnlyThreeActive_ReturnsFalse()
    {
        Participation[] participations = [P(ParticipationStatus.Reserved), P(ParticipationStatus.Reserved), P(ParticipationStatus.Paid)];

        Assert.False(RosterCompleteRule.HasFourActive(participations));
    }

    [Fact]
    public void IsComplete_FourPaid_ReturnsTrue()
    {
        Participation[] participations = [P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid)];

        Assert.True(RosterCompleteRule.IsComplete(participations));
    }

    [Fact]
    public void IsComplete_ThreePaidOneReserved_ReturnsFalse()
    {
        Participation[] participations = [P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Paid), P(ParticipationStatus.Reserved)];

        Assert.False(RosterCompleteRule.IsComplete(participations));
    }
}
