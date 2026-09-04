namespace Bll.Rules;

/// <summary>
/// RG-RES-001: a reservation can only be made once the member type's booking window has opened —
/// i.e. today is on or after (match date - lead time in days).
/// </summary>
public static class ReservationWindowRule
{
    public static bool IsWithinWindow(int windowDays, DateOnly matchDate, DateOnly today) =>
        today >= matchDate.AddDays(-windowDays);
}
