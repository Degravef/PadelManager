using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class TypeMembreConfiguration : IEntityTypeConfiguration<TypeMembre>
{
    public void Configure(EntityTypeBuilder<TypeMembre> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Libelle).IsRequired().HasMaxLength(50);
        builder.Property(t => t.PrefixeMatricule).IsRequired().HasMaxLength(1);
        builder.HasIndex(t => t.Code)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.TypeMembresCodeName);

        // DOMAIN_RULES.md §2 — booking-window lead time is data, not code (readme.md ERD hypothesis 5).
        builder.HasData(
            new TypeMembre
            {
                Id = TypeMembreSeed.GlobalId, Code = TypeMembreSeed.GlobalCode, Libelle = "Membre global",
                PrefixeMatricule = "G", DelaiReservationJours = 21
            },
            new TypeMembre
            {
                Id = TypeMembreSeed.SiteId, Code = TypeMembreSeed.SiteCode, Libelle = "Membre de site",
                PrefixeMatricule = "S", DelaiReservationJours = 14
            },
            new TypeMembre
            {
                Id = TypeMembreSeed.LibreId, Code = TypeMembreSeed.LibreCode, Libelle = "Membre libre",
                PrefixeMatricule = "L", DelaiReservationJours = 5
            });
    }
}
