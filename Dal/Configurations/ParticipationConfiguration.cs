using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class ParticipationConfiguration : IEntityTypeConfiguration<Participation>
{
    public void Configure(EntityTypeBuilder<Participation> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.AmountDue).HasPrecision(10, 2);
        builder.Property(p => p.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        // Optimistic concurrency: Status/PaymentDate are read-then-written by a payment and can
        // race the daily lifecycle batch deleting the same seat — see IConcurrencyToken.
        builder.Property(p => p.Version).IsConcurrencyToken();
        builder.HasOne(p => p.Match)
               .WithMany(m => m.Participations)
               .HasForeignKey(p => p.MatchId)
               .OnDelete(DeleteBehavior.Cascade); // deleting a Match removes its participations
        builder.HasOne(p => p.Member)
               .WithMany(m => m.Participations)
               .HasForeignKey(p => p.MemberId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => new { p.MatchId, p.MemberId })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.ParticipationsMatchIdMemberIdName);
        builder.HasIndex(p => new { p.MatchId, p.SeatNumber })
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.ParticipationsMatchIdSeatNumberName);

        // Forces the "at most 4 seats per match" structural rule into the DB: combined with the
        // unique (MatchId, SeatNumber) index above, a match can never accumulate a 5th distinct
        // seat row, even if application logic ever mis-computed a seat number.
        builder.ToTable(t => t.HasCheckConstraint("CK_Participations_SeatNumber_Range", "\"SeatNumber\" BETWEEN 1 AND 4"));
    }
}
