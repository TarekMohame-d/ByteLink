using System.Text.Json.Serialization;
using ByteLink.Api;
using ByteLink.Api.Extensions;
using ByteLink.Api.Middlewares;
using dotenv.net;
using Modules.Identity;
using Modules.Notifications;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Shared;
using Shared.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    DotEnv.Fluent().WithProbeForEnv(probeLevelsToSearch: 10).WithTrimValues().Load();
    builder.Configuration.AddEnvironmentVariables();
}

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);

// Add services to the container.
builder
    .Services.AddHostServices(builder.Configuration)
    .AddSharedServices(builder.Configuration)
    .AddIdentityModule(builder.Configuration)
    .AddNotificationsModule(builder.Configuration);

builder.Services.AddGlobalMessagingDecorators();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    // app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ByteLink API"));
}

app.UseMiddleware<RequestLogContextMiddleware>();

app.UseMiddleware<DistributedRateLimiterMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.00}ms";

    options.GetLevel = (httpContext, elapsedMs, ex) =>
    {
        if (ex is not null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var endpoint = httpContext.GetEndpoint();
        var routePattern = (endpoint as RouteEndpoint)?.RoutePattern.RawText;

        if (routePattern is not null)
        {
            diagnosticContext.Set("RequestPath", routePattern);
        }
    };
});

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.UseCustomHangfireDashboard(builder.Configuration);

app.UseHangfireJobs();

app.MapIdentityEndpoints();

// app.UseIdentityModuleBackgroundJobs(builder.Environment);

app.Run();
