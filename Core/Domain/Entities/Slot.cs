namespace Core.Domain.Entities;

public class Slot
{
    public int Id { get; set; }
    public required int SiteScheduleId { get; set; }
    public SiteSchedule? SiteSchedule { get; set; }
    public required int Order { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ICollection<Match> Matches { get; set; } = [];
}
