using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class MemberTypeConfiguration : IEntityTypeConfiguration<MemberType>
{
    public void Configure(EntityTypeBuilder<MemberType> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Label).IsRequired().HasMaxLength(50);
        builder.Property(t => t.MatriculePrefix).IsRequired().HasMaxLength(1);
        builder.HasIndex(t => t.Code)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.MemberTypesCodeName);

        // DOMAIN_RULES.md §2 — booking-window lead time is data, not code (readme.md ERD hypothesis 5).
        builder.HasData(
            new MemberType
            {
                Id = MemberTypeSeed.GlobalId, Code = MemberTypeSeed.GlobalCode, Label = "Membre global",
                MatriculePrefix = "G", ReservationWindowDays = 21
            },
            new MemberType
            {
                Id = MemberTypeSeed.SiteId, Code = MemberTypeSeed.SiteCode, Label = "Membre de site",
                MatriculePrefix = "S", ReservationWindowDays = 14
            },
            new MemberType
            {
                Id = MemberTypeSeed.LibreId, Code = MemberTypeSeed.LibreCode, Label = "Membre libre",
                MatriculePrefix = "L", ReservationWindowDays = 5
            });
    }
}
