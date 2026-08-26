namespace Core.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required int AdminId { get; set; }
}