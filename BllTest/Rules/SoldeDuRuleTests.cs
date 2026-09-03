using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace BllTest.Rules;

public class SoldeDuRuleTests
{
    [Fact]
    public void ADuSoldeImpaye_NoSoldes_ReturnsFalse()
    {
        Assert.False(SoldeDuRule.ADuSoldeImpaye([]));
    }

    [Fact]
    public void ADuSoldeImpaye_OnlyPaidSoldes_ReturnsFalse()
    {
        SoldeDu[] soldes = [new() { MembreId = 1, MatchId = 1, Statut = StatutSoldeDu.Paye }];

        Assert.False(SoldeDuRule.ADuSoldeImpaye(soldes));
    }

    [Fact]
    public void ADuSoldeImpaye_OneOutstandingSolde_ReturnsTrue()
    {
        SoldeDu[] soldes = [new() { MembreId = 1, MatchId = 1, Statut = StatutSoldeDu.Du }];

        Assert.True(SoldeDuRule.ADuSoldeImpaye(soldes));
    }
}
