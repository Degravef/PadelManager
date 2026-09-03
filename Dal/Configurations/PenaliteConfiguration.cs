using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class PenaliteConfiguration : IEntityTypeConfiguration<Penalite>
{
    public void Configure(EntityTypeBuilder<Penalite> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Motif).HasMaxLength(255);
        builder.HasOne(p => p.Membre)
               .WithMany(m => m.Penalites)
               .HasForeignKey(p => p.MembreId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Match)
               .WithMany(m => m.Penalites)
               .HasForeignKey(p => p.MatchId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => p.MembreId);
    }
}
