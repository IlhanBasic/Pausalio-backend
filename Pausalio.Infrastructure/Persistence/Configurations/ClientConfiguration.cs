using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");
            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Clients_IsActive",
                    "IsActive IN (0,1)"
                );

                t.HasCheckConstraint(
                    "CK_Clients_Dates",
                    "UpdatedAt IS NULL OR UpdatedAt >= CreatedAt"
                );
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.PIB)
                .HasMaxLength(20);

            builder.Property(x => x.MB)
                .HasMaxLength(20);

            builder.Property(x => x.Address)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.City)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => new { x.Email, x.BusinessProfileId }).IsUnique();
            builder.HasIndex(x => new { x.City, x.IsActive });
            builder.HasIndex(x => new { x.Name, x.BusinessProfileId });

            builder.HasOne(x => x.BusinessProfile)
                .WithMany(x => x.Clients)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Invoices)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
