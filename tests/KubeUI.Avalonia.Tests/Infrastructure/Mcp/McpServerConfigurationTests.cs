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
    public void is_port_available_reports_free_and_bound_ports()
    {
        var blocker = BlockPort(out var blockedPort);
        try
        {
            McpServerConfiguration.IsPortAvailable(blockedPort).ShouldBeFalse();

            var freePort = GetFreePort();
            McpServerConfiguration.IsPortAvailable(freePort).ShouldBeTrue();
        }
        finally
        {
            blocker.Dispose();
        }
    }

    [Fact]
    public void resolve_available_port_keeps_configured_port_when_free()
    {
        var port = GetFreePort();

        McpServerConfiguration.ResolveAvailablePort(port).ShouldBe(port);
    }

    [Fact]
    public void resolve_available_port_falls_back_to_ephemeral_when_configured_port_is_blocked()
    {
        var blocker = BlockPort(out var blockedPort);
        try
        {
            McpServerConfiguration.ResolveAvailablePort(blockedPort)
                .ShouldBe(McpServerConfiguration.EphemeralPort);
        }
        finally
        {
            blocker.Dispose();
        }
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static TcpListener BlockPort(out int port)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        port = ((IPEndPoint)listener.LocalEndpoint).Port;
        return listener;
    }
}
