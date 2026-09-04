namespace Bll.Rules;

/// RG-RES-001
public static class ReservationWindowRule
{
    public static bool IsWithinWindow(int windowDays, DateOnly matchDate, DateOnly today) =>
        today >= matchDate.AddDays(-windowDays);
}
