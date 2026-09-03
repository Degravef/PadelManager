using Core.Domain.Entities;

namespace Bll.Rules;


public static class JourOuvertRule
{
    public static bool EstOuvert(IEnumerable<JourFermeture> fermetures, DateOnly date) =>
        fermetures.All(f => f.DateFermeture != date);
}
