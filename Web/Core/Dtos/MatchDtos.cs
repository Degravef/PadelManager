namespace Web.Core.Dtos;

public record MatchDto(
    int Id,
    int TerrainId,
    DateOnly Date,
    TimeOnly StartTime,
    string TypeMatch,
    string Statut,
    int OrganisateurId,
    decimal MontantTotal);

public record CreerReservationDto(int TerrainId, DateOnly Date, TimeOnly StartTime, bool EstPublic = false);
