using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.TypeMatch).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Statut).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.MontantTotal).HasPrecision(10, 2);
        builder.Property(m => m.MontantPaye).HasPrecision(10, 2);
        builder.HasOne(m => m.Terrain)
               .WithMany(t => t.Matches)
               .HasForeignKey(m => m.TerrainId)
               .OnDelete(DeleteBehavior.Restrict); // ASSUMPTION: a Terrain with existing matches can't be deleted, not documented explicitly in DOMAIN_RULES.md
        builder.HasOne(m => m.Creneau)
               .WithMany(c => c.Matches)
               .HasForeignKey(m => m.CreneauId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Organisateur)
               .WithMany(mb => mb.MatchesOrganises)
               .HasForeignKey(m => m.OrganisateurId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(m => new { m.TerrainId, m.Date });
        builder.HasIndex(m => m.CreneauId);
        builder.HasIndex(m => m.OrganisateurId);
    }
}
