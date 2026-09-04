using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<SiteSchedule> SiteSchedules => Set<SiteSchedule>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<ClosureDay> ClosureDays => Set<ClosureDay>();
    public DbSet<MemberType> MemberTypes => Set<MemberType>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Participation> Participations => Set<Participation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BalanceDue> BalancesDue => Set<BalanceDue>();
    public DbSet<Penalty> Penalties => Set<Penalty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}
