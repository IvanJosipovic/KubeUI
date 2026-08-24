using System.Net;
using System.Net.Sockets;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Options;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpServerConfigurationTests
{
    [Fact]
    public void endpoint_uses_localhost_path_and_clamps_port()
    {
        var settings = new Settings { McpServerPort = 80 };
        McpServerConfiguration.GetEndpoint(settings).ShouldBe("http://127.0.0.1:1024/mcp");
    }

    [Fact]
    public void validation_preserves_ephemeral_port()
    {
        McpServerConfiguration.GetValidatedPort(McpServerConfiguration.EphemeralPort)
            .ShouldBe(McpServerConfiguration.EphemeralPort);
    }

    [Theory]
    [InlineData(80, 1024)]
    [InlineData(70000, 65535)]
    public void validation_clamps_ports_outside_the_allowed_range(int configuredPort, int expectedPort)
    {
        McpServerConfiguration.GetValidatedPort(configuredPort).ShouldBe(expectedPort);
    }

    [Fact]
    public void is_port_available_reports_free_and_blocked_ports()
    {
        using var blocker = new TcpListener(IPAddress.Loopback, 0);
        blocker.Start();
        var blockedPort = ((IPEndPoint)blocker.LocalEndpoint).Port;

        McpServerConfiguration.IsPortAvailable(blockedPort).ShouldBeFalse();

        var freePort = GetFreePort();
        McpServerConfiguration.IsPortAvailable(freePort).ShouldBeTrue();
    }

    [Fact]
    public void resolve_available_port_keeps_a_free_configured_port()
    {
        var port = GetFreePort();

        McpServerConfiguration.ResolveAvailablePort(port).ShouldBe(port);
    }

    [Fact]
    public void resolve_available_port_falls_back_to_ephemeral_when_configured_port_is_blocked()
    {
        using var blocker = new TcpListener(IPAddress.Loopback, 0);
        blocker.Start();
        var blockedPort = ((IPEndPoint)blocker.LocalEndpoint).Port;

        McpServerConfiguration.ResolveAvailablePort(blockedPort)
            .ShouldBe(McpServerConfiguration.EphemeralPort);
    }

    [Fact]
    public void resolve_available_port_resolves_ephemeral_port_to_ephemeral_port()
    {
        McpServerConfiguration.ResolveAvailablePort(McpServerConfiguration.EphemeralPort)
            .ShouldBe(McpServerConfiguration.EphemeralPort);
    }

    [Fact]
    public void server_state_tracks_the_live_endpoint()
    {
        McpServerState state = new();
        state.Endpoint.ShouldBeNull();

        state.SetEndpoint("http://127.0.0.1:54321/mcp");

        state.Endpoint.ShouldBe("http://127.0.0.1:54321/mcp");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void server_state_rejects_invalid_endpoints(string? endpoint)
    {
        var state = new McpServerState();

        Should.Throw<ArgumentException>(() => state.SetEndpoint(endpoint!));
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}

