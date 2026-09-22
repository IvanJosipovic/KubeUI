using KubeUI.Avalonia.Infrastructure.Mcp;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpServerStateTests
{
    [Fact]
    public void state_starts_without_a_bound_port()
    {
        var state = new McpServerState();

        state.BoundPort.ShouldBeNull();
    }

    [Fact]
    public void state_records_configured_port_after_successful_bind()
    {
        var state = new McpServerState();

        state.SetBoundPort(62888);

        state.BoundPort.ShouldBe(62888);
    }
}
