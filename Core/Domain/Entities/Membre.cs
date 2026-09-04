using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Membre
{
    public int Id { get; set; }
    public required string Matricule { get; set; }
    public required string Name { get; set; }
    public required string FirstName { get; set; }
    public required int TypeMembreId { get; set; }
    public TypeMembre? TypeMembre { get; set; }
    public int? SiteId { get; set; } // NULL si membre global ou libre
    public Site? Site { get; set; }
    // ASSUMPTION: nullable (ERD shows NOT NULL + unique) because CreateMembreDto collects it as
    // optional, not required — a member can register without one and add it later.
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public RoleMembre Role { get; set; } = RoleMembre.Joueur;
    public string? MotDePasseHash { get; set; } // NULL pour un joueur (pas de login)
    public DateOnly DateInscription { get; set; }
    public bool Actif { get; set; } = true;
    public ICollection<Match> MatchesOrganises { get; set; } = [];
    public ICollection<Participation> Participations { get; set; } = [];
    public ICollection<Paiement> Paiements { get; set; } = [];
    public ICollection<SoldeDu> SoldesDus { get; set; } = [];
    public ICollection<Penalite> Penalites { get; set; } = [];
}
