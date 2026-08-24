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
    public void dynamic_port_is_preserved_by_validation()
    {
        McpServerConfiguration.GetValidatedPort(McpServerConfiguration.DynamicPort)
            .ShouldBe(McpServerConfiguration.DynamicPort);
    }

    [Fact]
    public void configured_port_above_range_is_clamped()
    {
        McpServerConfiguration.GetValidatedPort(70000).ShouldBe(McpServerConfiguration.MaximumPort);
    }

    [Fact]
    public void endpoint_prefers_the_bound_port_over_the_configured_port()
    {
        var settings = new Settings { McpServerEnabled = true, McpServerPort = 62888 };

        McpServerConfiguration.GetEndpoint(settings, 54321).ShouldBe("http://127.0.0.1:54321/mcp");
    }

    [Fact]
    public void endpoint_falls_back_to_the_configured_port_without_a_bound_port()
    {
        var settings = new Settings { McpServerEnabled = true, McpServerPort = 62888 };

        McpServerConfiguration.GetEndpoint(settings, null).ShouldBe("http://127.0.0.1:62888/mcp");
    }

}
