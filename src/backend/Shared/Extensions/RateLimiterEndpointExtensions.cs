using Microsoft.AspNetCore.Builder;
using Shared.Infrastructure.Middlewares;

namespace Shared.Extensions;

public static class RateLimiterEndpointExtensions
{
    public static TBuilder RequireEmailLimit<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(new EmailLimitAttribute());
    }
}
