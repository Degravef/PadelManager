using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(10, 2);
        builder.Property(p => p.PaymentMethod).HasMaxLength(50);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.TransactionReference).HasMaxLength(100);
        builder.HasOne(p => p.Member)
               .WithMany(m => m.Payments)
               .HasForeignKey(p => p.MemberId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Participation)
               .WithMany(pa => pa.Payments)
               .HasForeignKey(p => p.ParticipationId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.BalanceDue)
               .WithMany(s => s.Payments)
               .HasForeignKey(p => p.BalanceDueId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => p.MemberId);
        builder.HasIndex(p => p.TransactionReference)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.PaymentsTransactionReferenceName);
    }
}
