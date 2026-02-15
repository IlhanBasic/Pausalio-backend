using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.ItemType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasIndex(x => x.BusinessProfileId);

            builder.HasOne(x => x.BusinessProfile)
                .WithMany(bp => bp.Items)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
