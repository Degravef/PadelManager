using Core.Constants;
using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>
/// RG-MEM-005/006/007: a Site-type member only sees/books/plays on their own site; Global and Libre
/// members act on any site.
/// </summary>
public static class PorteeMembreRule
{
    public static bool PeutAgirSurSite(Membre membre, int siteId) =>
        membre.TypeMembre!.Code != TypeMembreSeed.SiteCode || membre.SiteId == siteId;
}
