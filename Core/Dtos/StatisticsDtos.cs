namespace Core.Dtos;

// RG-PAY-009 / CF-RC-004.
public record RevenueDto(decimal Amount, DateOnly Start, DateOnly End);
