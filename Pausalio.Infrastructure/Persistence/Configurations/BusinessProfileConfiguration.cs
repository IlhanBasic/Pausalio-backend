using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class BusinessProfileConfiguration : IEntityTypeConfiguration<BusinessProfile>
    {
        public void Configure(EntityTypeBuilder<BusinessProfile> builder)
        {
            builder.ToTable("BusinessProfiles");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_BusinessProfiles_Dates",
                    "UpdatedAt IS NULL OR UpdatedAt >= CreatedAt"
                );
                t.HasCheckConstraint(
                    "CK_BusinessProfiles_IsActive",
                    "IsActive IN (0,1)"
                );
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BusinessName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.PIB)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.MB)
                .HasMaxLength(20);

            builder.Property(x => x.City)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Address)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.Website)
                .HasMaxLength(100);

            builder.Property(x => x.CompanyLogo)
                .HasMaxLength(200);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.PIB).IsUnique();
            builder.HasIndex(x => x.MB).IsUnique();
            builder.HasIndex(x => x.ActivityCodeId);
            builder.HasIndex(x => new { x.City, x.IsActive });

            builder.HasOne(b => b.ActivityCode)
                   .WithMany()
                   .HasForeignKey(b => b.ActivityCodeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.UserBusinessProfiles)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.BankAccounts)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Clients)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Invoices)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Payments)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Expenses)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.TaxObligations)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Documents)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Reminders)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.BusinessInvites)
                .WithOne(x => x.BusinessProfile)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
