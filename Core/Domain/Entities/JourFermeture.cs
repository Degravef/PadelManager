namespace Core.Domain.Entities;


public class JourFermeture
{
    public int Id { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public DateOnly DateFermeture { get; set; }
    public string Motif { get; set; } = string.Empty;
}
