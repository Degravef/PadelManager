namespace Web.Core.Dtos;

public record MatchDto(
    int Id,
    int TerrainId,
    DateOnly Date,
    TimeOnly StartTime,
    string TypeMatch,
    string Statut,
    int OrganisateurId,
    decimal MontantTotal,
    TimeOnly EndTime = default,
    decimal MontantPaye = 0m,
    DateTime DateCreation = default,
    DateOnly DateLimite = default,
    DateTime? DateBasculePublic = null);

public record CreerReservationDto(int TerrainId, DateOnly Date, TimeOnly StartTime, bool EstPublic = false);

public record AvailableSlotDto(int TerrainId, string TerrainName, TimeOnly StartTime, TimeOnly EndTime);
