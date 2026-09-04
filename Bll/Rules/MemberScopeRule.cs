using Core.Constants;
using Core.Domain.Entities;

namespace Bll.Rules;

/// RG-MEM-005/006/007
public static class MemberScopeRule
{
    public static bool CanActOnSite(Member member, int siteId) =>
        member.MemberType!.Code != MemberTypeSeed.SiteCode || member.SiteId == siteId;
}
