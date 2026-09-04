namespace Core.Dtos;

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

// EstPublic (RG-RES-005/CF-RV-018): the organizer declares the match Private (default) or Public at creation.
public record CreerReservationDto(int TerrainId, DateOnly Date, TimeOnly StartTime, bool EstPublic = false);

public record AvailableSlotDto(int TerrainId, string TerrainName, TimeOnly StartTime, TimeOnly EndTime);
