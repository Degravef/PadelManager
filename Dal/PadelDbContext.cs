using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Terrain> Terrains => Set<Terrain>();
    public DbSet<HoraireSite> HorairesSites => Set<HoraireSite>();
    public DbSet<Creneau> Creneaux => Set<Creneau>();
    public DbSet<JourFermeture> JoursFermeture => Set<JourFermeture>();
    public DbSet<TypeMembre> TypeMembres => Set<TypeMembre>();
    public DbSet<Membre> Membres => Set<Membre>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Participation> Participations => Set<Participation>();
    public DbSet<Paiement> Paiements => Set<Paiement>();
    public DbSet<SoldeDu> SoldesDus => Set<SoldeDu>();
    public DbSet<Penalite> Penalites => Set<Penalite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}
