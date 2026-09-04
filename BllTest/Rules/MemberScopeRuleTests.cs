using Bll.Rules;
using Core.Constants;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class MemberScopeRuleTests
{
    private static Member Member(string typeCode, int? siteId) => new()
    {
        Matricule = "X1", Name = "N", FirstName = "F", MemberTypeId = 1, SiteId = siteId,
        MemberType = new MemberType { Code = typeCode, Label = typeCode, MatriculePrefix = typeCode[0].ToString(), ReservationWindowDays = 1 }
    };

    [Fact]
    public void CanActOnSite_SiteMemberOnOwnSite_ReturnsTrue()
    {
        Assert.True(MemberScopeRule.CanActOnSite(Member(MemberTypeSeed.SiteCode, 5), 5));
    }

    [Fact]
    public void CanActOnSite_SiteMemberOnOtherSite_ReturnsFalse()
    {
        Assert.False(MemberScopeRule.CanActOnSite(Member(MemberTypeSeed.SiteCode, 5), 6));
    }

    [Fact]
    public void CanActOnSite_GlobalMemberOnAnySite_ReturnsTrue()
    {
        Assert.True(MemberScopeRule.CanActOnSite(Member(MemberTypeSeed.GlobalCode, null), 6));
    }

    [Fact]
    public void CanActOnSite_LibreMemberOnAnySite_ReturnsTrue()
    {
        Assert.True(MemberScopeRule.CanActOnSite(Member(MemberTypeSeed.LibreCode, null), 6));
    }
}
