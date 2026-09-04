namespace Core.Domain.Entities;

// Opening hours for a Site, defined per calendar year. Unique on (SiteId, Year).
public class SiteSchedule
{
    public int Id { get; set; }
    public required int SiteId { get; set; }
    public Site? Site { get; set; }
    public required int Year { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public int MatchDurationMinutes { get; set; } = 90;
    public int BreakMinutes { get; set; } = 15;
    public int RequiredPlayers { get; set; } = 4;
    public decimal MatchPrice { get; set; } = 60m;
    public ICollection<Slot> Slots { get; set; } = [];
}
