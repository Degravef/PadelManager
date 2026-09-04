namespace Core.Dtos;

public record MatchDto(
    int Id,
    int CourtId,
    DateOnly Date,
    TimeOnly StartTime,
    string Type,
    string Status,
    int OrganizerId,
    decimal TotalAmount,
    TimeOnly EndTime = default,
    decimal AmountPaid = 0m,
    DateTime CreatedAt = default,
    DateOnly PaymentDeadline = default,
    DateTime? PublicSwitchDate = null);

// IsPublic (RG-RES-005/CF-RV-018): the organizer declares the match Private (default) or Public at creation.
public record CreateReservationDto(int CourtId, DateOnly Date, TimeOnly StartTime, bool IsPublic = false);

public record AvailableSlotDto(int CourtId, string CourtName, TimeOnly StartTime, TimeOnly EndTime);
