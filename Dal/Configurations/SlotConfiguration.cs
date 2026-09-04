using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasOne(c => c.SiteSchedule)
               .WithMany(h => h.Slots)
               .HasForeignKey(c => c.SiteScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => new { c.SiteScheduleId, c.Order })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.SlotsSiteScheduleIdOrderName);
    }
}
