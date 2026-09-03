using Core.Domain.Entities;

namespace Bll.Rules;

/// <summary>RG-SITE-007/008: a reservation can't be made on a closure day, site-specific or global.</summary>
public static class JourOuvertRule
{
    public static bool EstOuvert(IEnumerable<JourFermeture> fermetures, DateOnly date) =>
        fermetures.All(f => f.DateFermeture != date);
}
