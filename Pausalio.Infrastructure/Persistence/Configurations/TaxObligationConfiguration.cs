using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class TaxObligationConfiguration : IEntityTypeConfiguration<TaxObligation>
    {
        public void Configure(EntityTypeBuilder<TaxObligation> builder)
        {
            builder.ToTable("TaxObligations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Year)
                .IsRequired();

            builder.Property(x => x.Month)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.PaidDate)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => x.PaymentId);

            builder.HasOne(x => x.Payment)
                   .WithOne(p => p.TaxObligation)
                   .HasForeignKey<TaxObligation>(x => x.PaymentId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
