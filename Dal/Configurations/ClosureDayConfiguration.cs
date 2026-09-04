using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class ClosureDayConfiguration : IEntityTypeConfiguration<ClosureDay>
{
    public void Configure(EntityTypeBuilder<ClosureDay> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Reason).HasMaxLength(255);
        builder.HasOne(j => j.Site)
               .WithMany(s => s.ClosureDays)
               .HasForeignKey(j => j.SiteId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(j => j.SiteId);
        builder.HasIndex(j => j.ClosureDate);
    }
}
