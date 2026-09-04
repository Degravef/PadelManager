using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Member
{
    public int Id { get; set; }
    public required string Matricule { get; set; }
    public required string Name { get; set; }
    public required string FirstName { get; set; }
    public required int MemberTypeId { get; set; }
    public MemberType? MemberType { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Player;
    public string? PasswordHash { get; set; }
    public DateOnly RegistrationDate { get; set; }
    public bool Active { get; set; } = true;
    public ICollection<Match> OrganizedMatches { get; set; } = [];
    public ICollection<Participation> Participations { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<BalanceDue> BalancesDue { get; set; } = [];
    public ICollection<Penalty> Penalties { get; set; } = [];
}
