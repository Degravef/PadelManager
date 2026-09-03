using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class SoldeOrganisateurRuleTests
{
    private static Participation P(StatutParticipation statut, decimal montant = 15m) => new() { MatchId = 1, NumeroPlace = 1, Statut = statut, MontantDu = montant };

    [Fact]
    public void CalculerSolde_NoOnePaid_ReturnsFullAmount()
    {
        Assert.Equal(60m, SoldeOrganisateurRule.CalculerSolde(60m, []));
    }

    [Fact]
    public void CalculerSolde_TwoPaidTwoUnpaid_ReturnsHalfAmount()
    {
        Participation[] participations = [P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Reservee), P(StatutParticipation.Reservee)];

        Assert.Equal(30m, SoldeOrganisateurRule.CalculerSolde(60m, participations));
    }

    [Fact]
    public void CalculerSolde_AllFourPaid_ReturnsZero()
    {
        Participation[] participations = [P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee), P(StatutParticipation.Payee)];

        Assert.Equal(0m, SoldeOrganisateurRule.CalculerSolde(60m, participations));
    }
}
