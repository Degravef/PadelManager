using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class PaiementConfiguration : IEntityTypeConfiguration<Paiement>
{
    public void Configure(EntityTypeBuilder<Paiement> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Montant).HasPrecision(10, 2);
        builder.Property(p => p.MoyenPaiement).HasMaxLength(50);
        builder.Property(p => p.Statut).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ReferenceTransaction).HasMaxLength(100);
        builder.HasOne(p => p.Membre)
               .WithMany(m => m.Paiements)
               .HasForeignKey(p => p.MembreId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Participation)
               .WithMany(pa => pa.Paiements)
               .HasForeignKey(p => p.ParticipationId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.SoldeDu)
               .WithMany(s => s.Paiements)
               .HasForeignKey(p => p.SoldeDuId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => p.MembreId);
        builder.HasIndex(p => p.ReferenceTransaction)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.PaiementsReferenceTransactionName);
    }
}
