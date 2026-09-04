using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>RG-SITE-007/008: a reservation can't be made on a closure day, site-specific or global.</summary>
public static class SiteOpenRule
{
    public static bool IsOpen(IEnumerable<ClosureDay> closureDays, DateOnly date) =>
        closureDays.All(f => f.ClosureDate != date);
}
