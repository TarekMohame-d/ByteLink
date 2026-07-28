using Hangfire;
using Shared.Infrastructure.Messaging.Jobs;

namespace ByteLink.Api.Extensions;

public static class HangfireJobs
{
    public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
    {
        RecurringJob.AddOrUpdate<IOutboxInboxCleanupService>(
            recurringJobId: "outbox-inbox-cleanup",
            methodCall: job => job.ProcessCleanupAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 2)
        );

        return app;
    }
}
