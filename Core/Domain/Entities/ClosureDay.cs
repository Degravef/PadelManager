namespace Core.Domain.Entities;

// A closure day. SiteId = null means a global closure (all sites — see DOMAIN_RULES.md §5).
public class ClosureDay
{
    public int Id { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public DateOnly ClosureDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
