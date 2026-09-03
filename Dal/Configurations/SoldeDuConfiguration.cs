using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class SoldeDuConfiguration : IEntityTypeConfiguration<SoldeDu>
{
    public void Configure(EntityTypeBuilder<SoldeDu> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Montant).HasPrecision(10, 2);
        builder.Property(s => s.Statut).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.HasOne(s => s.Membre)
               .WithMany(m => m.SoldesDus)
               .HasForeignKey(s => s.MembreId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Match)
               .WithMany(m => m.SoldesDus)
               .HasForeignKey(s => s.MatchId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => s.MembreId);
        builder.HasIndex(s => s.MatchId);
    }
}
