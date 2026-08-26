using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Terrain> Terrains => Set<Terrain>();
    public DbSet<Membre> Membres => Set<Membre>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Participation> Participations => Set<Participation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}