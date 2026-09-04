namespace Web.Core.Dtos;

public record MembreDto(
    int Id,
    string Matricule,
    string Name,
    string FirstName,
    string TypeMembre,
    int? SiteId,
    decimal SoldeDu,
    DateTime? DateFinPenalite,
    string? Email = null,
    string? Telephone = null,
    string Role = "Joueur",
    DateOnly DateInscription = default,
    bool Actif = true);

// Type is "GLOBAL" / "SITE" / "LIBRE", matching MembreDto.TypeMembre. The matricule itself is
// server-generated from it, not supplied here.
public record CreateMembreDto(string Name, string FirstName, string Type, int? SiteId, string? Email = null, string? Telephone = null);
