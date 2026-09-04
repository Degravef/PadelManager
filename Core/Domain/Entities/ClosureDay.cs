namespace Core.Domain.Entities;

public class ClosureDay
{
    public int Id { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public DateOnly ClosureDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
