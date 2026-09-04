using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class PenaltyConfiguration : IEntityTypeConfiguration<Penalty>
{
    public void Configure(EntityTypeBuilder<Penalty> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Reason).HasMaxLength(255);
        builder.HasOne(p => p.Member)
               .WithMany(m => m.Penalties)
               .HasForeignKey(p => p.MemberId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Match)
               .WithMany(m => m.Penalties)
               .HasForeignKey(p => p.MatchId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => p.MemberId);
    }
}
