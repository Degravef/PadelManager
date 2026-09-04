namespace Core.Dtos;

public record MemberDto(
    int Id,
    string Matricule,
    string Name,
    string FirstName,
    string MemberType,
    int? SiteId,
    decimal BalanceDue,
    DateTime? PenaltyEndDate,
    string? Email = null,
    string? Phone = null,
    string Role = "Player",
    DateOnly RegistrationDate = default,
    bool Active = true);

public record CreateMemberDto(string Name, string FirstName, string Type, int? SiteId, string? Email = null, string? Phone = null);
