namespace Core.Dtos;

// Summary of one J-1 daily batch run (RG-ETA-002/003).
public record DailyBatchResultDto(
    DateOnly ProcessedDate,
    int MatchesSwitchedIncompleteRoster,
    int MatchesSwitchedUnpaidSeat,
    int PenaltiesApplied,
    int BalancesDueCreated,
    int MatchesCompleted);
