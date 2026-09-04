using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.SiteId).IsRequired();
        builder.Property(t => t.Number).HasMaxLength(20);
        builder.Property(t => t.SurfaceType).HasMaxLength(50);
        builder.HasOne(t => t.Site)
               .WithMany(s => s.Courts)
               .HasForeignKey(t => t.SiteId)
               .OnDelete(DeleteBehavior.Cascade); // ASSUMPTION: deleting a Site removes its Courts, not documented explicitly in DOMAIN_RULES.md
        builder.HasIndex(t => t.SiteId);
        builder.HasIndex(t => new { t.SiteId, t.Name })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.CourtsSiteIdName);
        builder.HasIndex(t => new { t.SiteId, t.Number })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.CourtsSiteIdNumberName);
    }
}
