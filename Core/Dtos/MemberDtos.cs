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

// Type is one of Core.Constants.MemberTypeSeed's codes (GLOBAL/SITE/LIBRE) — kept as a plain string
// rather than an enum to stay consistent with MemberDto.MemberType and match how Web (which can't
// reference Core) represents it. The matricule itself is server-generated, not supplied here.
public record CreateMemberDto(string Name, string FirstName, string Type, int? SiteId, string? Email = null, string? Phone = null);
