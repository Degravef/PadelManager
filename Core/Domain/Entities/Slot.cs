namespace Core.Domain.Entities;

// A mechanically-derived slot (90 + 15 min) for a SiteSchedule. Unique on (SiteScheduleId, Order).
public class Slot
{
    public int Id { get; set; }
    public required int SiteScheduleId { get; set; }
    public SiteSchedule? SiteSchedule { get; set; }
    public required int Order { get; set; } // 1, 2, 3 ...
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ICollection<Match> Matches { get; set; } = [];
}
