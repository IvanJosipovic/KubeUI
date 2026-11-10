using KubeUI.Features.AI.Mcp;
using Shouldly;

namespace KubeUI.Features.AI.Tests.Mcp;

public sealed class McpServerConfigurationTests
{
    [Fact]
    public void endpoint_uses_configured_localhost_port()
    {
        McpServerConfiguration.GetEndpoint(62888).ShouldBe("http://127.0.0.1:62888/mcp");
    }
}
