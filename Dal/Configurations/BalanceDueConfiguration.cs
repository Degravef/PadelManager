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
        builder.HasIndex(s => s.MatchId)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.BalancesDueMatchIdName);
    }
}
