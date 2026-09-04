using Core.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Matricule).IsRequired().HasMaxLength(6);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Email).HasMaxLength(255);
        builder.Property(m => m.Phone).HasMaxLength(30);
        builder.Property(m => m.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.PasswordHash).HasMaxLength(255);
        builder.HasOne(m => m.MemberType)
               .WithMany(t => t.Members)
               .HasForeignKey(m => m.MemberTypeId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Site)
               .WithMany(s => s.Members)
               .HasForeignKey(m => m.SiteId)
               .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(m => m.SiteId);
        builder.HasIndex(m => m.MemberTypeId);
        builder.HasIndex(m => m.Matricule)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.MembersMatriculeName);
        builder.HasIndex(m => m.Email)
               .IsUnique()
               .HasDatabaseName(ConstraintsNames.MembersEmailName);
    }
}
