using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal;

public class SiteEntityConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.HasKey(s => s.Name);
        builder.Property(s => s.Name).IsRequired();
        builder.Property(s => s.Address).IsRequired();
        builder.Property(s => s.AdminId).IsRequired();
        builder.HasIndex(s => new { s.AdminId, s.Name }).IsUnique();
    }
}