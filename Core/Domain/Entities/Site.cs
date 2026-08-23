namespace Core.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }

    // Navigation properties can be added later as other entities are implemented
}