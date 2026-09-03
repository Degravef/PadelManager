using Bll.Rules;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class JourOuvertRuleTests
{
    [Fact]
    public void EstOuvert_NoClosures_ReturnsTrue()
    {
        Assert.True(JourOuvertRule.EstOuvert([], new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void EstOuvert_ClosureOnDifferentDate_ReturnsTrue()
    {
        JourFermeture[] fermetures = [new() { SiteId = 1, DateFermeture = new DateOnly(2026, 9, 11) }];

        Assert.True(JourOuvertRule.EstOuvert(fermetures, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void EstOuvert_SiteSpecificClosureOnDate_ReturnsFalse()
    {
        JourFermeture[] fermetures = [new() { SiteId = 1, DateFermeture = new DateOnly(2026, 9, 10) }];

        Assert.False(JourOuvertRule.EstOuvert(fermetures, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void EstOuvert_GlobalClosureOnDate_ReturnsFalse()
    {
        JourFermeture[] fermetures = [new() { SiteId = null, DateFermeture = new DateOnly(2026, 9, 10) }];

        Assert.False(JourOuvertRule.EstOuvert(fermetures, new DateOnly(2026, 9, 10)));
    }
}
