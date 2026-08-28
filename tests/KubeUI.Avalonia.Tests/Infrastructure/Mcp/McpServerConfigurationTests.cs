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

}
