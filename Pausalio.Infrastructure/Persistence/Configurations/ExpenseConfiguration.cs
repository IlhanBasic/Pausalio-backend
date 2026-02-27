using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");
            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Expenses_IsDeleted",
                    "IsDeleted IN (0,1)"
                );

                t.HasCheckConstraint(
                    "CK_Expenses_Dates",
                    "DeletedAt IS NULL OR DeletedAt >= CreatedAt"
                );

                t.HasCheckConstraint(
                    "CK_Expenses_Status",
                    "Status IN (1,2,3)"
                );
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(50);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => x.ReferenceNumber).IsUnique(false);
            builder.HasIndex(x => new { x.BusinessProfileId, x.IsDeleted });

            builder.HasOne(x => x.BusinessProfile)
                .WithMany(x => x.Expenses)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
