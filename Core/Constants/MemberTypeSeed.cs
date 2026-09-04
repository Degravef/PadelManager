namespace Core.Constants;

// Well-known MemberType rows, seeded via EF Core HasData (see Dal/Configurations/MemberTypeConfiguration.cs).
// Ids are referenced directly by tests that need a valid MemberTypeId FK without a DB round-trip.
public static class MemberTypeSeed
{
    public const int GlobalId = 1;
    public const int SiteId = 2;
    public const int LibreId = 3;

    public const string GlobalCode = "GLOBAL";
    public const string SiteCode = "SITE";
    public const string LibreCode = "LIBRE";
}
