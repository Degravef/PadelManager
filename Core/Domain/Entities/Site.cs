namespace Core.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required int AdminId { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool Actif { get; set; } = true;
    public ICollection<Terrain> Terrains { get; set; } = [];
    public ICollection<HoraireSite> HorairesSites { get; set; } = [];
    public ICollection<JourFermeture> JoursFermeture { get; set; } = [];
    public ICollection<Membre> Membres { get; set; } = [];
}
