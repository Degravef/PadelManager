using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class EffectifCompletRuleTests
{
    private static Participation P(StatutParticipation statut) => new() { MatchId = 1, NumeroPlace = 1, Statut = statut };

    [Fact]
    public void AQuatreActifs_ThreeReserveesOnePayee_ReturnsTrue()
    {
        Participation[] participations = [P(StatutParticipation.Reservee), P(StatutParticipation.Reservee), P(StatutParticipation.Reservee), P(StatutParticipation.Payee)];

        Assert.True(EffectifCompletRule.AQuatreActifs(participations));
    }

    [Fact]
    public void AQuatreActifs_OnlyThreeActive_ReturnsFalse()
    {
        Participation[] participations = [P(StatutParticipation.Reservee), P(StatutParticipation.Reservee), P(StatutParticipation.Payee)];

        Assert.False(EffectifCompletRule.AQuatreActifs(participations));
    }

    [Fact]
    public void EstComplet_FourPayees_ReturnsTrue()
    {
        Participation[] participations = [P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee)];

        Assert.True(EffectifCompletRule.EstComplet(participations));
    }

    [Fact]
    public void EstComplet_ThreePayeesOneReservee_ReturnsFalse()
    {
        Participation[] participations = [P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Reservee)];

        Assert.False(EffectifCompletRule.EstComplet(participations));
    }
}
