namespace Core.Dtos;

public record SiteScheduleDto(
    int Id,
    int SiteId,
    int Year,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    int MatchDurationMinutes,
    int BreakMinutes,
    decimal MatchPrice,
    int RequiredPlayers = 4);

public record CreateSiteScheduleDto(
    int Year,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    decimal? MatchPrice = null);
