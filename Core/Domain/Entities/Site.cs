namespace Core.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required int AdminId { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool Active { get; set; } = true;
    public ICollection<Court> Courts { get; set; } = [];
    public ICollection<SiteSchedule> SiteSchedules { get; set; } = [];
    public ICollection<ClosureDay> ClosureDays { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
}
