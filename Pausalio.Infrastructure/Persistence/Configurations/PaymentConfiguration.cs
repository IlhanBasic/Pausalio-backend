using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.AmountRSD)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.ExchangeRate)
                .HasColumnType("decimal(18,4)");

            builder.Property(x => x.Currency)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.PaymentDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder.Property(x => x.PaymentType)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => x.InvoiceId);
            builder.HasIndex(x => x.TaxObligationId);
            builder.HasIndex(x => x.ExpenseId);

            builder.HasOne(x => x.BusinessProfile)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.TaxObligation)
                .WithOne()
                .HasForeignKey<Payment>(x => x.TaxObligationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Expense)
                .WithOne()
                .HasForeignKey<Payment>(x => x.ExpenseId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
