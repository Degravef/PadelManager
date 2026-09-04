namespace Web.Core.Dtos;

public record DailyBatchResultDto(
    DateOnly ProcessedDate,
    int MatchesSwitchedIncompleteRoster,
    int MatchesSwitchedUnpaidSeat,
    int PenaltiesApplied,
    int BalancesDueCreated,
    int MatchesCompleted);
