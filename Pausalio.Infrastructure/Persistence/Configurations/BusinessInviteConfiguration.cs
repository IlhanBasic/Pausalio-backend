using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class BusinessInviteConfiguration : IEntityTypeConfiguration<BusinessInvite>
    {
        public void Configure(EntityTypeBuilder<BusinessInvite> builder)
        {
            builder.ToTable("BusinessInvites");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(x => x.Token)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(x => x.ExpiresAt)
                   .IsRequired();

            builder.Property(x => x.IsUsed)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("UTC_TIMESTAMP()")
                   .IsRequired();

            builder.HasIndex(x => x.Token)
                   .IsUnique();

            builder.HasIndex(x => new { x.BusinessProfileId, x.Email });

            builder.HasOne(x => x.BusinessProfile)
                   .WithMany(b => b.BusinessInvites)
                   .HasForeignKey(x => x.BusinessProfileId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
