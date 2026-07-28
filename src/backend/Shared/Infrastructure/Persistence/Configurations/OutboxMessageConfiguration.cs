using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");

        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventId).ValueGeneratedNever();

        builder.Property(x => x.Type).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.ProcessedAtUtc);
        builder.Property(x => x.Error);
        builder.Property(x => x.RetryCount).IsRequired();
    }
}
