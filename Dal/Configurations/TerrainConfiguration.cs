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
        builder.Property(t => t.Numero).HasMaxLength(20);
        builder.Property(t => t.TypeSurface).HasMaxLength(50);
        builder.HasOne(t => t.Site)
               .WithMany(s => s.Terrains)
               .HasForeignKey(t => t.SiteId)
               .OnDelete(DeleteBehavior.Cascade); 
        builder.HasIndex(t => t.SiteId);
        builder.HasIndex(t => new { t.SiteId, t.Name })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.TerrainsSiteIdName);
        builder.HasIndex(t => new { t.SiteId, t.Numero })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.TerrainsSiteIdNumeroName);
    }
}
