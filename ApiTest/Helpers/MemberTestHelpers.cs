namespace ApiTest.Helpers;

internal static class MemberTestHelpers
{
    // Mirrors Core.Constants.MemberTypeSeed's codes/prefixes; kept here (rather than referenced
    // directly) since these tests exercise the public API contract, not Core internals.
    public static string TypeFromPrefix(char prefix) => prefix switch
    {
        'G' => "GLOBAL",
        'S' => "SITE",
        'L' => "LIBRE",
        _ => throw new ArgumentOutOfRangeException(nameof(prefix))
    };
}
