namespace Core.Constants;

// Well-known TypeMembre rows, seeded via EF Core HasData (see Dal/Configurations/TypeMembreConfiguration.cs).
// Ids are referenced directly by tests that need a valid TypeMembreId FK without a DB round-trip.
public static class TypeMembreSeed
{
    public const int GlobalId = 1;
    public const int SiteId = 2;
    public const int LibreId = 3;

    public const string GlobalCode = "GLOBAL";
    public const string SiteCode = "SITE";
    public const string LibreCode = "LIBRE";
}
