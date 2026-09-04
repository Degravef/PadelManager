using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.TotalAmount).HasPrecision(10, 2);
        builder.Property(m => m.AmountPaid).HasPrecision(10, 2);
        builder.Property(m => m.Version).IsConcurrencyToken();
        builder.HasOne(m => m.Court)
               .WithMany(t => t.Matches)
               .HasForeignKey(m => m.CourtId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Slot)
               .WithMany(c => c.Matches)
               .HasForeignKey(m => m.SlotId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Organizer)
               .WithMany(mb => mb.OrganizedMatches)
               .HasForeignKey(m => m.OrganizerId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(m => new { m.CourtId, m.Date });
        builder.HasIndex(m => m.SlotId);
        builder.HasIndex(m => m.OrganizerId);

        // RG-SITE-006
        builder.HasIndex(m => new { m.CourtId, m.Date, m.StartTime })
               .IsUnique()
               .HasFilter("\"Status\" <> 'Cancelled'")
               .HasDatabaseName(ConstraintsNames.MatchesCourtIdDateStartTimeName);
    }
}
