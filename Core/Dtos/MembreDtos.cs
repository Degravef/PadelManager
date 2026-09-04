namespace Core.Dtos;

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

// Type is one of Core.Constants.TypeMembreSeed's codes (GLOBAL/SITE/LIBRE) — kept as a plain string
// rather than an enum to stay consistent with MembreDto.TypeMembre and match how Web (which can't
// reference Core) represents it. The matricule itself is server-generated, not supplied here.
public record CreateMembreDto(string Name, string FirstName, string Type, int? SiteId, string? Email = null, string? Telephone = null);
