using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Membre
{
    public int Id { get; set; }
    public required string Matricule { get; set; }
    public required string Name { get; set; }
    public required string FirstName { get; set; }
    public required TypeMembre TypeMembre { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public decimal SoldeDu { get; set; }
    public DateTime? DateFinPenalite { get; set; }
}
