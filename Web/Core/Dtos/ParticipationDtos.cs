namespace Web.Core.Dtos;

public record ParticipationDto(
    int Id,
    int MatchId,
    int? MemberId,
    int SeatNumber,
    string Role,
    string Status,
    decimal AmountDue,
    DateTime RegistrationDate,
    DateTime? PaymentDate);

public record AddPlayerDto(string Matricule);
