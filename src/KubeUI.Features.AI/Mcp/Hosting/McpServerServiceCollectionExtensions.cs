using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.RateLimiting;
using KubeUI.Features.AI.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;

namespace KubeUI.Features.AI.Mcp.Hosting;

/// <summary>Registers the secured loopback MCP server used by a KubeUI AI host.</summary>
public static class McpServerServiceCollectionExtensions
{
    /// <summary>
    /// Adds MCP transport, authentication, request filtering, and a host-owned listener.
    /// </summary>
    /// <typeparam name="TTools">The MCP tool type registered by the host application.</typeparam>
    /// <param name="services">Services to configure.</param>
    /// <param name="port">Loopback port used by the endpoint.</param>
    /// <param name="toolSerializerOptions">Source-generated serializer metadata for tool inputs and results.</param>
    /// <param name="createHostedService">Creates the host-specific listener service.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKubeUIMcpServer<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods)] TTools>(
        this IServiceCollection services,
        int port,
        JsonSerializerOptions toolSerializerOptions,
        Func<IServiceProvider, int, IHostedService> createHostedService)
        where TTools : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(toolSerializerOptions);
        ArgumentNullException.ThrowIfNull(createHostedService);

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
                            PermitLimit = McpServerRateLimits.MaximumConcurrentRequests,
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
                            PermitLimit = McpServerRateLimits.MaximumRequestsPerMinutePerAddress,
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
            options.AddPolicy(McpServerRateLimits.ClientRateLimitPolicyName, context =>
            {
                var identity = context.RequestServices
                    .GetRequiredService<McpRequestIdentityAccessor>().Current;
                return RateLimitPartition.GetSlidingWindowLimiter(
                    identity?.Id ?? "unauthenticated",
                    static _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = McpServerRateLimits.MaximumRequestsPerMinute,
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
            .WithTools<TTools>(toolSerializerOptions)
            .WithRequestFilters(McpToolAuditFilter.Configure);
        services.AddSingleton<IHostedService>(serviceProvider =>
            createHostedService(serviceProvider, port));

        return services;
    }
}
