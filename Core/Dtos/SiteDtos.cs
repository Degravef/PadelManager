namespace Core.Dtos;

public record SiteDto(int Id, string Name, string Address);

public record CreateSiteDto(string Name, string Address);

public record UpdateSiteDto(string Name, string Address);