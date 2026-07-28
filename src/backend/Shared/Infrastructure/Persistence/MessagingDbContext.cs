using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Persistence;

public sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options) : DbContext(options)
{
    public const string Schema = "messaging";
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessagingDbContext).Assembly);
    }
}
