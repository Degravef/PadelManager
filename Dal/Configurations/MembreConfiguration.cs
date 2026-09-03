using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class MembreConfiguration : IEntityTypeConfiguration<Membre>
{
    public void Configure(EntityTypeBuilder<Membre> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Matricule).IsRequired().HasMaxLength(6);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Email).HasMaxLength(255);
        builder.Property(m => m.Telephone).HasMaxLength(30);
        builder.Property(m => m.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.MotDePasseHash).HasMaxLength(255);
        builder.HasOne(m => m.TypeMembre)
               .WithMany(t => t.Membres)
               .HasForeignKey(m => m.TypeMembreId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Site)
               .WithMany(s => s.Membres)
               .HasForeignKey(m => m.SiteId)
               .OnDelete(DeleteBehavior.SetNull); 
        builder.HasIndex(m => m.SiteId);
        builder.HasIndex(m => m.TypeMembreId);
        builder.HasIndex(m => m.Matricule)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.MembresMatriculeName);
        builder.HasIndex(m => m.Email)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.MembresEmailName);
    }
}
