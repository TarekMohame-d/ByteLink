using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "messaging");

        builder.HasKey(x => new { x.EventId, x.ConsumerName });

        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.ConsumerName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ProcessedAtUtc);
    }
}
