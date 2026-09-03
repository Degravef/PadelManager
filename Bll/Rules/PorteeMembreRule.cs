using Core.Constants;
using Core.Domain.Entities;

namespace Bll.Rules;





public static class PorteeMembreRule
{
    public static bool PeutAgirSurSite(Membre membre, int siteId) =>
        membre.TypeMembre!.Code != TypeMembreSeed.SiteCode || membre.SiteId == siteId;
}
