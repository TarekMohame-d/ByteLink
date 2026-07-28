using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Configurations;

namespace Shared.Infrastructure.Messaging.Extensions;

public static class MessagingDbContextExtensions
{
    public static void ApplyMessagingConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }
}
