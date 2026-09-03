namespace ApiTest.Helpers;

internal static class MembreTestHelpers
{
    
    
    public static string TypeFromPrefix(char prefix) => prefix switch
    {
        'G' => "GLOBAL",
        'S' => "SITE",
        'L' => "LIBRE",
        _ => throw new ArgumentOutOfRangeException(nameof(prefix))
    };
}
