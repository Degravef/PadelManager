using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class TerrainConfiguration : IEntityTypeConfiguration<Terrain>
{
    public void Configure(EntityTypeBuilder<Terrain> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.SiteId).IsRequired();
        builder.HasOne(t => t.Site)
               .WithMany()
               .HasForeignKey(t => t.SiteId)
               .OnDelete(DeleteBehavior.Cascade); // ASSUMPTION: deleting a Site removes its Terrains, not documented explicitly in DOMAIN_RULES.md
        builder.HasIndex(t => t.SiteId);
        builder.HasIndex(t => new { t.SiteId, t.Name })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.TerrainsSiteIdName);
    }
}
