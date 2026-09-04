using Bll.Rules;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class SiteOpenRuleTests
{
    [Fact]
    public void IsOpen_NoClosures_ReturnsTrue()
    {
        Assert.True(SiteOpenRule.IsOpen([], new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void IsOpen_ClosureOnDifferentDate_ReturnsTrue()
    {
        ClosureDay[] closures = [new() { SiteId = 1, ClosureDate = new DateOnly(2026, 9, 11) }];

        Assert.True(SiteOpenRule.IsOpen(closures, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void IsOpen_SiteSpecificClosureOnDate_ReturnsFalse()
    {
        ClosureDay[] closures = [new() { SiteId = 1, ClosureDate = new DateOnly(2026, 9, 10) }];

        Assert.False(SiteOpenRule.IsOpen(closures, new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void IsOpen_GlobalClosureOnDate_ReturnsFalse()
    {
        ClosureDay[] closures = [new() { SiteId = null, ClosureDate = new DateOnly(2026, 9, 10) }];

        Assert.False(SiteOpenRule.IsOpen(closures, new DateOnly(2026, 9, 10)));
    }
}
