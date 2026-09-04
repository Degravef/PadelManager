namespace Core.Dtos;

public record SiteDto(
    int Id,
    string Name,
    string Address,
    string? PostalCode = null,
    string? City = null,
    string? Phone = null,
    string? Email = null,
    bool Active = true);

public record CreateSiteDto(
    string Name,
    string Address,
    string? PostalCode = null,
    string? City = null,
    string? Phone = null,
    string? Email = null);

public record UpdateSiteDto(
    string Name,
    string Address,
    string? PostalCode = null,
    string? City = null,
    string? Phone = null,
    string? Email = null,
    bool Active = true);
