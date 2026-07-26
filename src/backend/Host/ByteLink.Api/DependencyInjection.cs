using System.Data.Common;
using System.Diagnostics;
using ByteLink.Api.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Savorboard.CAP.InMemoryMessageQueue;
using Shared.Constants;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace ByteLink.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostBuilder host
    )
    {
        services.AddExceptionHandling();

        // services.AddHangfireBackgroundJobs(configuration);

        services.AddApplicationResilience();

        services.AddHttpContextAccessor();

        services.AddCache(configuration);

        services.AddCAP(configuration);

        return services;
    }

    private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(configure =>
        {
            configure.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;

                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails();

        return services;
    }

    private static IServiceCollection AddHangfireBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(config =>
        {
            config.UsePostgreSqlStorage(
                options =>
                    options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",

                    // optionally tune other settings:
                    // QueuePollInterval = TimeSpan.FromSeconds(15),
                    // InvisibilityTimeout = TimeSpan.FromMinutes(30),
                    // TablePrefix = "hf_",
                }
            );
        });
        services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

        return services;
    }

    private static IServiceCollection AddApplicationResilience(this IServiceCollection services) =>
        services.AddResiliencePipeline(
            ResilienceConstants.StandardPolicy,
            builder =>
            {
                builder.AddRetry(
                    new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder().Handle<Exception>(),

                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromMilliseconds(50),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                    }
                );

                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions
                    {
                        // If 50% of requests fail...
                        FailureRatio = 0.5,

                        // ...within a 30-second window...
                        SamplingDuration = TimeSpan.FromSeconds(30),

                        // ...and we have attempted at least 7 requests...
                        MinimumThroughput = 7,

                        // ...then stop all requests for 15 seconds.
                        BreakDuration = TimeSpan.FromSeconds(15),

                        // Handle DB Concurrency, but also generic DB Exceptions (timeouts, connection issues)
                        ShouldHandle = new PredicateBuilder()
                            .Handle<DbUpdateConcurrencyException>()
                            .Handle<DbUpdateException>() // Catch general EF errors
                            .Handle<TimeoutException>()
                            .Handle<DbException>()
                            .Handle<NpgsqlException>()
                            .Handle<TimeoutException>(),
                    }
                );
            }
        );

    private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        var valkeyConnectionString = configuration.GetConnectionString("Valkey")!;

        var valkeyConnection = ConnectionMultiplexer.Connect(valkeyConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(valkeyConnection);

        services.AddStackExchangeRedisCache(options =>
        {
            options.InstanceName = "Bytelink:";
            options.ConnectionMultiplexerFactory = () =>
                Task.FromResult<IConnectionMultiplexer>(valkeyConnection);
        });

        services
            .AddFusionCache()
            .WithOptions(options =>
            {
                options.DefaultEntryOptions = new FusionCacheEntryOptions
                {
                    Duration = TimeSpan.FromMinutes(10),
                    DistributedCacheDuration = TimeSpan.FromMinutes(10),
                    JitterMaxDuration = TimeSpan.FromSeconds(10),
                    IsFailSafeEnabled = true,
                    FailSafeMaxDuration = TimeSpan.FromHours(1),
                    FailSafeThrottleDuration = TimeSpan.FromSeconds(30),
                    EagerRefreshThreshold = 0.9f,
                };
            })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithRegisteredDistributedCache()
            .WithBackplane(
                new RedisBackplane(
                    new RedisBackplaneOptions
                    {
                        ConnectionMultiplexerFactory = () =>
                            Task.FromResult<IConnectionMultiplexer>(valkeyConnection),
                    }
                )
            );

        return services;
    }

    private static IServiceCollection AddCAP(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            options.UseEntityFramework<IdentityDbContext>();

            options.UsePostgreSql(opt =>
            {
                opt.ConnectionString = configuration.GetConnectionString("DefaultConnection");
                opt.Schema = "cap";
            });

            // options.UseInMemoryMessageQueue();
            options.UseDashboard(path => path.PathMatch = "/cap-dashboard");

            options.UseRabbitMQ(rabbit =>
            {
                rabbit.HostName = configuration["RabbitMQ:HostName"]!;
                rabbit.Port = int.Parse(configuration["RabbitMQ:Port"]!);
                rabbit.UserName = configuration["RabbitMQ:UserName"]!;
                rabbit.Password = configuration["RabbitMQ:Password"]!;
                rabbit.VirtualHost = configuration["RabbitMQ:VirtualHost"]!;

                rabbit.ExchangeName = "cap.default.router";

                rabbit.ConnectionFactoryOptions = opt =>
                {
                    opt.AutomaticRecoveryEnabled = true;
                    opt.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
                };
            });

            options.ConsumerThreadCount = 2;
        });

        return services;
    }
}
