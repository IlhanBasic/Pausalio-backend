using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("BankAccounts");
            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_BankAccounts_AccountOrIBAN",
                    "(AccountNumber IS NOT NULL AND CHAR_LENGTH(AccountNumber) > 0) OR (IBAN IS NOT NULL AND CHAR_LENGTH(IBAN) > 0)"
                );

                t.HasCheckConstraint(
                    "CK_BankAccounts_Dates",
                    "UpdatedAt IS NULL OR UpdatedAt >= CreatedAt"
                );
            });

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.BusinessProfile)
                .WithMany()
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.BankName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.AccountNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.IBAN)
                .HasMaxLength(34);

            builder.Property(x => x.SWIFT)
                .HasMaxLength(11);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => x.AccountNumber)
                .IsUnique();
            builder.HasIndex(x => x.IBAN)
                .IsUnique();
            builder.HasIndex(x => new { x.BusinessProfileId, x.IsActive });
        }
    }
}
