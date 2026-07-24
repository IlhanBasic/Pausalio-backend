using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pausalio.Domain.Entities;

namespace Pausalio.Infrastructure.Persistence.Configurations
{
    internal class AiToolCallConfiguration : IEntityTypeConfiguration<AiToolCall>
    {
        public void Configure(EntityTypeBuilder<AiToolCall> builder)
        {
            builder.ToTable("aitoolcalls");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasColumnType("char(36)");

            builder.Property(x => x.MessageId)
                   .HasColumnType("char(36)")
                   .IsRequired();

            builder.Property(x => x.ToolName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(x => x.Arguments)
                   .HasColumnType("longtext")
                   .IsRequired();

            builder.Property(x => x.Result)
                   .HasColumnType("longtext");

            builder.Property(x => x.Success)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.Property(x => x.ErrorMessage)
                   .HasMaxLength(1000);

            builder.Property(x => x.RoundNumber)
                   .IsRequired();

            builder.Property(x => x.DurationMs)
                   .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                   .IsRequired();
        }
    }
}