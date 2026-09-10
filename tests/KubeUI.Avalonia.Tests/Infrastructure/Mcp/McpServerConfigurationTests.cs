using KubeUI.Avalonia.Infrastructure.Mcp;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpServerConfigurationTests
{
    [Fact]
    public void endpoint_uses_configured_localhost_port()
    {
        var settings = new Settings { McpServerPort = 62888 };
        McpServerConfiguration.GetEndpoint(settings).ShouldBe("http://127.0.0.1:62888/mcp");
    }
}
