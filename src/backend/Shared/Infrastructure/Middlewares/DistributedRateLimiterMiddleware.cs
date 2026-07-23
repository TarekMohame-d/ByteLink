using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Shared.Infrastructure.Middlewares;

public sealed class DistributedRateLimiterMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private static readonly LuaScript _rateLimitScript = LuaScript.Prepare(
        @"
        local count = redis.call('INCR', @key)
        if count == 1 then
            redis.call('EXPIRE', @key, @windowSeconds)
        end
        return count
    "
    );

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        string policyName = "global";
        int maxRequests = 100;
        int windowSeconds = 60;

        if (endpoint?.Metadata.GetMetadata<EmailLimitAttribute>() is not null)
        {
            policyName = "email";
            maxRequests = 1;
        }

        string timeSlot = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        RedisKey redisKey = $"rl:{policyName}:{ipAddress}:{timeSlot}";

        var db = redis.GetDatabase();
        var result = await db.ScriptEvaluateAsync(_rateLimitScript, new { key = redisKey, windowSeconds });
        long currentCount = (long)result;

        if (currentCount > maxRequests)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,

                Title = "Too Many Requests",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Detail =
                    $"Rate limit exceeded for the '{policyName}' policy. Maximum allowed is {maxRequests} requests per minute.",
                Extensions = new Dictionary<string, object?>
                {
                    { "errorCode", "RateLimitExceeded" },
                    {
                        "errorDescription",
                        $"Rate limit exceeded. Please try again after {windowSeconds} seconds."
                    },
                },
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        await next(context);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class EmailLimitAttribute : Attribute;
