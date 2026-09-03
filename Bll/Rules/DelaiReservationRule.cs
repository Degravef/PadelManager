namespace Bll.Rules;





public static class DelaiReservationRule
{
    public static bool EstDansLaFenetre(int delaiJours, DateOnly dateMatch, DateOnly aujourdHui) =>
        aujourdHui >= dateMatch.AddDays(-delaiJours);
}
