using System.Net.Sockets;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Desktop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpHostStartupTests
{
    [Fact]
    public void bind_failure_is_detected_through_wrapped_socket_exceptions()
    {
        var exception = new IOException(
            "Failed to bind to address http://127.0.0.1:62888.",
            new AggregateException(
                new SocketException(),
                new SocketException()));

        Program.IsPortBindFailure(exception).ShouldBeTrue();
        Program.IsPortBindFailure(new SocketException()).ShouldBeTrue();
        Program.IsPortBindFailure(new InvalidOperationException("unrelated")).ShouldBeFalse();
    }

    [Fact]
    public void bound_port_is_recorded_from_loopback_server_address()
    {
        var services = new ServiceCollection()
            .AddSingleton<McpServerState>()
            .BuildServiceProvider();

        Program.RecordMcpBoundPort(services, ["http://127.0.0.1:54321"]);

        services.GetRequiredService<McpServerState>().BoundPort.ShouldBe(54321);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-uri")]
    [InlineData("http://example.com:54321")]
    public void unbindable_addresses_do_not_record_a_bound_port(string? address)
    {
        var services = new ServiceCollection()
            .AddSingleton<McpServerState>()
            .BuildServiceProvider();

        Program.RecordMcpBoundPort(services, [address]);

        services.GetRequiredService<McpServerState>().BoundPort.ShouldBeNull();
    }

    [Fact]
    public void recording_bound_port_without_registered_state_or_addresses_is_ignored()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        Should.NotThrow(() => Program.RecordMcpBoundPort(services, ["http://127.0.0.1:54321"]));

        var stateServices = new ServiceCollection()
            .AddSingleton<McpServerState>()
            .BuildServiceProvider();

        Should.NotThrow(() => Program.RecordMcpBoundPort(stateServices, null));
        stateServices.GetRequiredService<McpServerState>().BoundPort.ShouldBeNull();
    }
}
