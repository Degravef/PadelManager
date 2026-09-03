using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Address).IsRequired().HasMaxLength(255);
        builder.Property(s => s.AdminId).IsRequired();
        builder.Property(s => s.PostalCode).HasMaxLength(20);
        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(255);
        builder.HasIndex(s => s.AdminId);
        builder.HasIndex(s => new { s.AdminId, s.Name })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.SitesAdminIdName);
    }
}
