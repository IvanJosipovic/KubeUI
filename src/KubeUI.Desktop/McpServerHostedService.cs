using System.Net;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KubeUI.Desktop;

internal sealed class McpServerHostedService(
    IServiceProvider desktopServices,
    int port) : IHostedService, IAsyncDisposable
{
    internal const string ClientRateLimitPolicyName = "KubeUI.Mcp.Client";
    internal const int MaximumConcurrentRequests = 16;
    internal const int MaximumRequestsPerMinute = 120;
    internal const int MaximumRequestsPerMinutePerAddress = 600;

    private readonly IServiceProvider _desktopServices = desktopServices;
    private readonly int _port = port;
    private readonly ILogger<McpServerHostedService> _logger =
        desktopServices.GetRequiredService<ILogger<McpServerHostedService>>();
    private KestrelServer? _server;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var serverOptions = new KestrelServerOptions
        {
            ApplicationServices = _desktopServices,
            Limits =
            {
                MaxRequestBodySize = 1024 * 1024,
                RequestHeadersTimeout = TimeSpan.FromSeconds(10)
            }
        };
        serverOptions.Listen(IPAddress.Loopback, _port);

        var loggerFactory = _desktopServices.GetRequiredService<ILoggerFactory>();
        var server = new KestrelServer(
            Options.Create(serverOptions),
            new SocketTransportFactory(Options.Create(new SocketTransportOptions()), loggerFactory),
            loggerFactory);

        var applicationBuilder = new ApplicationBuilder(_desktopServices);
        applicationBuilder.UseHostFiltering();
        // Bound invalid credentials before routing; the limiter validates credentials before selecting partitions.
        applicationBuilder.UseRateLimiter();
        applicationBuilder.UseMiddleware<McpSecurityMiddleware>(_port);
        applicationBuilder.UseRouting();
        // The second pass applies the endpoint's per-client policy after authentication.
        applicationBuilder.UseRateLimiter();
        applicationBuilder.UseEndpoints(endpoints =>
            endpoints.MapMcp(McpServerConfiguration.Path)
                .RequireRateLimiting(ClientRateLimitPolicyName));

        var application = new KestrelHttpApplication(
            applicationBuilder.Build(),
            _desktopServices.GetRequiredService<IServiceScopeFactory>());

        try
        {
            await server.StartAsync(application, cancellationToken).ConfigureAwait(false);
            _server = server;
            _desktopServices.GetRequiredService<McpServerState>().SetBoundPort(_port);
        }
        catch (Exception exception) when (Program.IsPortBindFailure(exception))
        {
            server.Dispose();
            _desktopServices.GetRequiredService<McpServerState>().SetBoundPort(null);
            _logger.LogWarning(
                exception,
                "The configured MCP server port {Port} could not be bound; continuing without MCP.",
                _port);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_server is null)
            return;

        await _server.StopAsync(cancellationToken).ConfigureAwait(false);
        _server.Dispose();
        _server = null;
        _desktopServices.GetRequiredService<McpServerState>().SetBoundPort(null);
    }

    public ValueTask DisposeAsync()
    {
        if (_server is null)
            return ValueTask.CompletedTask;

        _server.Dispose();
        _server = null;
        _desktopServices.GetRequiredService<McpServerState>().SetBoundPort(null);
        return ValueTask.CompletedTask;
    }

    private sealed class KestrelHttpApplication(
        RequestDelegate requestDelegate,
        IServiceScopeFactory scopeFactory) : IHttpApplication<RequestContext>
    {
        public RequestContext CreateContext(IFeatureCollection features)
        {
            var scope = scopeFactory.CreateScope();
            var httpContext = new DefaultHttpContext(features)
            {
                RequestServices = scope.ServiceProvider
            };
            return new RequestContext(httpContext, scope);
        }

        public Task ProcessRequestAsync(RequestContext context)
        {
            return requestDelegate(context.HttpContext);
        }

        public void DisposeContext(RequestContext context, Exception? exception)
        {
            context.Scope.Dispose();
        }
    }

    private sealed record RequestContext(
        DefaultHttpContext HttpContext,
        IServiceScope Scope);
}
