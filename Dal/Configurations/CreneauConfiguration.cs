using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class CreneauConfiguration : IEntityTypeConfiguration<Creneau>
{
    public void Configure(EntityTypeBuilder<Creneau> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.HoraireSite)
               .WithMany(h => h.Creneaux)
               .HasForeignKey(c => c.HoraireSiteId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => new { c.HoraireSiteId, c.Ordre })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.CreneauxHoraireSiteIdOrdreName);
    }
}
