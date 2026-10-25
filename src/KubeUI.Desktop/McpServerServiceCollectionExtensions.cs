using System.Diagnostics;
using System.Threading.RateLimiting;
using KubeUI.AI.Mcp;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;

namespace KubeUI.Desktop;

internal static class McpServerServiceCollectionExtensions
{
    internal static IServiceCollection AddKubeUIMcpServer(this IServiceCollection services, int port)
    {
        services.AddHostFiltering(options =>
        {
            options.AllowedHosts.Clear();
            options.AllowedHosts.Add(McpServerConfiguration.Host);
            options.AllowedHosts.Add(McpServerConfiguration.Localhost);
            options.AllowEmptyHosts = false;
            options.IncludeFailureMessage = false;
        });
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // Authenticated clients skip these pre-authentication partitions. Invalid or missing
            // credentials share global and address limits; the endpoint pass applies client limits.
            var concurrencyLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var identity = context.RequestServices
                    .GetRequiredService<McpRequestIdentityAccessor>().Current;
                identity ??= McpSecurityMiddleware.GetRequestIdentity(
                    context,
                    context.RequestServices.GetRequiredService<IMcpCredentialService>(),
                    port);
                return identity is null
                    ? RateLimitPartition.GetConcurrencyLimiter(
                        "KubeUI.Mcp.Global",
                        static _ => new ConcurrencyLimiterOptions
                        {
                            PermitLimit = McpServerHostedService.MaximumConcurrentRequests,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        })
                    : RateLimitPartition.GetNoLimiter(identity.Id);
            });
            var preAuthenticationRateLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var identity = context.RequestServices
                    .GetRequiredService<McpRequestIdentityAccessor>().Current;
                identity ??= McpSecurityMiddleware.GetRequestIdentity(
                    context,
                    context.RequestServices.GetRequiredService<IMcpCredentialService>(),
                    port);
                return identity is null
                    ? RateLimitPartition.GetSlidingWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        static _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = McpServerHostedService.MaximumRequestsPerMinutePerAddress,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        })
                    : RateLimitPartition.GetNoLimiter(identity.Id);
            });
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                concurrencyLimiter,
                preAuthenticationRateLimiter);
            options.AddPolicy(McpServerHostedService.ClientRateLimitPolicyName, context =>
            {
                var identity = context.RequestServices
                    .GetRequiredService<McpRequestIdentityAccessor>().Current;
                return RateLimitPartition.GetSlidingWindowLimiter(
                    identity?.Id ?? "unauthenticated",
                    static _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = McpServerHostedService.MaximumRequestsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });
        });
        services.AddRouting();
        services.AddSingleton(static _ => new DiagnosticListener("KubeUI.Mcp"));
        services.AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true)
            .WithListResourcesHandler(static (_, _) =>
                ValueTask.FromResult(new ListResourcesResult { Resources = [] }))
            .WithListResourceTemplatesHandler(static (_, _) =>
                ValueTask.FromResult(new ListResourceTemplatesResult { ResourceTemplates = [] }))
            .WithTools<McpTools>(McpToolJsonSerializationContext.CreateSerializerOptions())
            .WithRequestFilters(McpToolAuditFilter.Configure);
        services.AddSingleton<IHostedService>(serviceProvider =>
            new McpServerHostedService(serviceProvider, port));

        return services;
    }
}
