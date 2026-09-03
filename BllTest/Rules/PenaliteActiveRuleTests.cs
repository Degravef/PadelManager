using Bll.Rules;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class PenaliteActiveRuleTests
{
    [Fact]
    public void EstActive_NoPenalites_ReturnsFalse()
    {
        Assert.False(PenaliteActiveRule.EstActive([], new DateOnly(2026, 9, 10)));
    }

    [Fact]
    public void EstActive_InactiveFlag_ReturnsFalse()
    {
        Penalite[] penalites = [new() { MembreId = 1, DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = false }];

        Assert.False(PenaliteActiveRule.EstActive(penalites, new DateOnly(2026, 9, 5)));
    }

    [Fact]
    public void EstActive_TodayBeforeEndDate_ReturnsTrue()
    {
        Penalite[] penalites = [new() { MembreId = 1, DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = true }];

        Assert.True(PenaliteActiveRule.EstActive(penalites, new DateOnly(2026, 9, 5)));
    }

    [Fact]
    public void EstActive_TodayAfterEndDate_ReturnsFalse()
    {
        Penalite[] penalites = [new() { MembreId = 1, DateDebut = new DateOnly(2026, 9, 1), DateFin = new DateOnly(2026, 9, 8), Active = true }];

        Assert.False(PenaliteActiveRule.EstActive(penalites, new DateOnly(2026, 9, 9)));
    }
}
