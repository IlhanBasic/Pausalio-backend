using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder.Property(x => x.DeliveredAt)
                .IsRequired(false);

            builder.Property(x => x.ReadAt)
                .IsRequired(false);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasIndex(x => new { x.SenderId, x.ReceiverId, x.BusinessProfileId });
            builder.HasIndex(x => new { x.ReceiverId, x.BusinessProfileId, x.Status });
            builder.HasIndex(x => x.SentAt);

            builder.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BusinessProfile)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}