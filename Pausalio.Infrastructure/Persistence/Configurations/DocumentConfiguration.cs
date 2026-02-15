using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using Pausalio.Shared.Enums;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");
            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Documents_IsDeleted",
                    "IsDeleted IN (0,1)"
                );

                t.HasCheckConstraint(
                    "CK_Documents_Dates",
                    "DeletedAt IS NULL OR DeletedAt >= UploadedAt"
                );
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.DocumentType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.UploadedAt)
                .HasDefaultValueSql("UTC_TIMESTAMP()")
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.BusinessProfileId);
            builder.HasIndex(x => x.DocumentNumber).IsUnique();
            builder.HasIndex(x => new { x.BusinessProfileId, x.IsDeleted });

            builder.HasOne(x => x.BusinessProfile)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
