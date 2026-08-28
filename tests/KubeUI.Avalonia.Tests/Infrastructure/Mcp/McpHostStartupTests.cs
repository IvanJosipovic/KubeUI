using System.Net.Sockets;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Desktop;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpHostStartupTests
{
    [Fact]
    public void bind_failure_is_detected_through_wrapped_socket_exceptions()
    {
        var exception = new IOException(
            "Failed to bind to address http://localhost:62888.",
            new AggregateException([new SocketException(10013), new SocketException(10013)]));

        Program.IsPortBindFailure(exception).ShouldBeTrue();
        Program.IsPortBindFailure(new SocketException(10013)).ShouldBeTrue();
        Program.IsPortBindFailure(new InvalidOperationException("unrelated")).ShouldBeFalse();
    }

    [Theory]
    [InlineData("http://127.0.0.1:54321")]
    [InlineData("http://localhost:54321")]
    [InlineData("http://[::1]:54321")]
    public void bound_port_is_recorded_from_a_loopback_server_address(string address)
    {
        var state = new McpServerState();

        Program.RecordMcpBoundPort(state, [address]);

        state.BoundPort.ShouldBe(54321);
    }

    [Fact]
    public void the_first_loopback_address_wins()
    {
        var state = new McpServerState();

        Program.RecordMcpBoundPort(state, ["http://[::1]:60000", "http://127.0.0.1:60000"]);

        state.BoundPort.ShouldBe(60000);
    }

    [Fact]
    public void addresses_without_a_usable_loopback_port_are_skipped()
    {
        var state = new McpServerState();

        Program.RecordMcpBoundPort(state, [null, "", "not-a-uri", "ftp://127.0.0.1:54321", "http://example.com:8080"]);

        state.BoundPort.ShouldBeNull();
    }

    [Fact]
    public void recording_the_bound_port_tolerates_missing_state_or_addresses()
    {
        Should.NotThrow(() => Program.RecordMcpBoundPort(null, ["http://127.0.0.1:54321"]));
        Should.NotThrow(() => Program.RecordMcpBoundPort(new McpServerState(), null));
    }
}
