using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class SiteScheduleConfiguration : IEntityTypeConfiguration<SiteSchedule>
{
    public void Configure(EntityTypeBuilder<SiteSchedule> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.MatchPrice).HasPrecision(10, 2);
        builder.HasOne(h => h.Site)
               .WithMany(s => s.SiteSchedules)
               .HasForeignKey(h => h.SiteId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(h => new { h.SiteId, h.Year })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.SiteSchedulesSiteIdYearName);
    }
}
