namespace Core.Domain.Entities;

// A closure day. SiteId = null means a global closure (all sites — see DOMAIN_RULES.md §5).
public class JourFermeture
{
    public int Id { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public DateOnly DateFermeture { get; set; }
    public string Motif { get; set; } = string.Empty;
}
