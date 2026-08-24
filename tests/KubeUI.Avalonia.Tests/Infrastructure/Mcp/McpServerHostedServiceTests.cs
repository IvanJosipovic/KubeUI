using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using KubeUI.AI.Permissions;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Desktop;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpServerHostTests
{
    [Fact]
    public async Task enabled_mcp_host_accepts_initialize_and_stops_cleanly()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var builder = Program.CreateHostBuilder([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);
        using var application = Program.CreateAndConfigureMcpEndpoint(builder);

        try
        {
            await application.StartAsync(CancellationToken.None);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2025-06-18",
                    capabilities = new { },
                    clientInfo = new { name = "KubeUI.Tests", version = "1.0" }
                }
            });

            using var response = await client.SendAsync(request);
            response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync());
            var body = await response.Content.ReadAsStringAsync();
            body.ShouldContain("serverInfo");
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task enabled_mcp_host_invokes_registered_tools()
    {
        var port = GetAvailablePort();
        var settings = new Settings
        {
            McpServerEnabled = true,
            McpServerPort = port
        };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var clusterCatalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        var navigation = new Mock<IResourceNavigationService>(MockBehavior.Strict);
        navigation.Setup(service => service.OpenResourceListAsync("dev", "apps/v1", "Deployment"))
            .ReturnsAsync(true);
        clusterCatalog.Setup(catalog => catalog.GetDefault()).Returns((IClusterRuntime?)null);
        clusterCatalog.SetupGet(catalog => catalog.Clusters).Returns([]);
        var builder = Program.CreateHostBuilder([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
            services.Replace(ServiceDescriptor.Singleton(clusterCatalog.Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(navigation.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);
        using var application = Program.CreateAndConfigureMcpEndpoint(builder);

        try
        {
            await application.StartAsync(CancellationToken.None);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, McpServerConfiguration.GetEndpoint(settings));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "tools/call",
                @params = new
                {
                    name = "kubeui_open_resource_list",
                    arguments = new { cluster = "dev", apiVersion = "apps/v1", kind = "Deployment" }
                }
            });

            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            response.IsSuccessStatusCode.ShouldBeTrue(body);
            body.ShouldNotContain("\"isError\":true");
            body.ShouldContain("true");
            navigation.VerifyAll();
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task host_startup_falls_back_to_an_os_assigned_port_when_the_configured_port_is_blocked()
    {
        using var ipv4Blocker = StartListener(System.Net.IPAddress.Loopback);
        using var ipv6Blocker = StartListener(System.Net.IPAddress.IPv6Loopback);
        var blockedPort = ((System.Net.IPEndPoint)ipv4Blocker.LocalEndpoint).Port;
        var settings = new Settings { McpServerEnabled = true, McpServerPort = blockedPort };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        using var application = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IMcpClusterSession>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object));
            services.Replace(ServiceDescriptor.Singleton(new Mock<IAgentPermissionService>(MockBehavior.Strict).Object));
        }, mcpPortOverride: blockedPort, mcpEnabledOverride: true);

        try
        {
            var state = application.Services.GetRequiredService<IMcpServerState>();
            state.BoundPort.ShouldNotBeNull();
            state.BoundPort.Value.ShouldBeGreaterThan(0);
            state.BoundPort.Value.ShouldNotBe(blockedPort);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"http://{McpServerConfiguration.Host}:{state.BoundPort}{McpServerConfiguration.Path}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
            request.Content = JsonContent.Create(new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2025-06-18",
                    capabilities = new { },
                    clientInfo = new { name = "KubeUI.Tests", version = "1.0" }
                }
            });

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
            response.IsSuccessStatusCode.ShouldBeTrue(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain("serverInfo");
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task host_startup_records_the_bound_port_when_the_configured_port_is_available()
    {
        var port = GetAvailablePort();
        var settings = new Settings { McpServerEnabled = true, McpServerPort = port };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        using var application = Program.CreateStartedHost([], includeOptionalServices: false, configureServices: services =>
        {
            services.Replace(ServiceDescriptor.Singleton(settingsService.Object));
        }, mcpPortOverride: port, mcpEnabledOverride: true);

        try
        {
            application.Services.GetRequiredService<IMcpServerState>()
                .BoundPort.ShouldBe(port);
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task disabled_mcp_host_does_not_claim_a_fixed_default_port()
    {
        using var application = Program.CreateStartedHost([], includeOptionalServices: false, mcpEnabledOverride: false);

        try
        {
            var addresses = application.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()?.Addresses ?? [];

            addresses.ShouldNotContain(address => address.Contains(":5000", StringComparison.Ordinal));
        }
        finally
        {
            await application.StopAsync(CancellationToken.None);
        }
    }

    private static TcpListener StartListener(System.Net.IPAddress address)
    {
        var listener = new TcpListener(address, 0);
        listener.Start();
        return listener;
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }
}
