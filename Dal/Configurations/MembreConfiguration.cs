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
        builder.Property(m => m.TypeMembre).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.SoldeDu).HasPrecision(10, 2);
        builder.HasOne(m => m.Site)
               .WithMany()
               .HasForeignKey(m => m.SiteId)
               .OnDelete(DeleteBehavior.SetNull); // ASSUMPTION: deleting a Site keeps its former members around (Global/Libre members aren't tied to any site), not documented explicitly in DOMAIN_RULES.md
        builder.HasIndex(m => m.SiteId);
        builder.HasIndex(m => m.Matricule)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.MembresMatriculeName);
    }
}
