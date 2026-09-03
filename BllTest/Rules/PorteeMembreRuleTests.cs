using Bll.Rules;
using Core.Constants;
using Core.Domain.Entities;

namespace BllTest.Rules;

public class PorteeMembreRuleTests
{
    private static Membre Membre(string typeCode, int? siteId) => new()
    {
        Matricule = "X1", Name = "N", FirstName = "F", TypeMembreId = 1, SiteId = siteId,
        TypeMembre = new TypeMembre { Code = typeCode, Libelle = typeCode, PrefixeMatricule = typeCode[0].ToString(), DelaiReservationJours = 1 }
    };

    [Fact]
    public void PeutAgirSurSite_SiteMemberOnOwnSite_ReturnsTrue()
    {
        Assert.True(PorteeMembreRule.PeutAgirSurSite(Membre(TypeMembreSeed.SiteCode, 5), 5));
    }

    [Fact]
    public void PeutAgirSurSite_SiteMemberOnOtherSite_ReturnsFalse()
    {
        Assert.False(PorteeMembreRule.PeutAgirSurSite(Membre(TypeMembreSeed.SiteCode, 5), 6));
    }

    [Fact]
    public void PeutAgirSurSite_GlobalMemberOnAnySite_ReturnsTrue()
    {
        Assert.True(PorteeMembreRule.PeutAgirSurSite(Membre(TypeMembreSeed.GlobalCode, null), 6));
    }

    [Fact]
    public void PeutAgirSurSite_LibreMemberOnAnySite_ReturnsTrue()
    {
        Assert.True(PorteeMembreRule.PeutAgirSurSite(Membre(TypeMembreSeed.LibreCode, null), 6));
    }
}
