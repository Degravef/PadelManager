using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public required int TerrainId { get; set; }
    public Terrain? Terrain { get; set; }
    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TypeMatch TypeMatch { get; set; }
    public required StatutMatch Statut { get; set; }
    public required int OrganisateurId { get; set; }
    public Membre? Organisateur { get; set; }
    public decimal MontantTotal { get; set; }
}
