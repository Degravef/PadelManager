using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class HoraireSiteConfiguration : IEntityTypeConfiguration<HoraireSite>
{
    public void Configure(EntityTypeBuilder<HoraireSite> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.PrixMatch).HasPrecision(10, 2);
        builder.HasOne(h => h.Site)
               .WithMany(s => s.HorairesSites)
               .HasForeignKey(h => h.SiteId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(h => new { h.SiteId, h.Annee })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.HorairesSitesSiteIdAnneeName);
    }
}
