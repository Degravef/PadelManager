using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}