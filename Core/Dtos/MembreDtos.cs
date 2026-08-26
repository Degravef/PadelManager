namespace Core.Dtos;

public record MembreDto(
    int Id,
    string Matricule,
    string Name,
    string FirstName,
    string TypeMembre,
    int? SiteId,
    decimal SoldeDu,
    DateTime? DateFinPenalite);

public record CreateMembreDto(string Name, string FirstName, int? SiteId);
