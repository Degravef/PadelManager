using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class BalanceDueConfiguration : IEntityTypeConfiguration<BalanceDue>
{
    public void Configure(EntityTypeBuilder<BalanceDue> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Amount).HasPrecision(10, 2);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        // Optimistic concurrency: Status/SettlementDate can be written by PayParticipationAsync
        // (folding the balance in) and PayBalanceDueAsync at the same time — see IConcurrencyToken.
        builder.Property(s => s.Version).IsConcurrencyToken();
        builder.HasOne(s => s.Member)
               .WithMany(m => m.BalancesDue)
               .HasForeignKey(s => s.MemberId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Match)
               .WithMany(m => m.BalancesDue)
               .HasForeignKey(s => s.MatchId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => s.MemberId);
        // MatchLifecycleService.ExecuteDailyBatchAsync only ever creates one BalanceDue per match
        // (it checks GetByMatchIdAsync first) — forced into the DB so two concurrent batch runs
        // can't both slip past that check and create a duplicate for the same match.
        builder.HasIndex(s => s.MatchId)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.BalancesDueMatchIdName);
    }
}
