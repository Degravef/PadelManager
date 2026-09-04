namespace Core.Dtos;

public record PaymentDto(
    int Id,
    int MemberId,
    int? ParticipationId,
    int? BalanceDueId,
    decimal Amount,
    DateTime PaymentDate,
    string Status);

public record PayDto(string? PaymentMethod = null);

public record BalanceDueDto(int Id, int MemberId, int MatchId, decimal Amount, string Status, DateTime CreatedAt);
