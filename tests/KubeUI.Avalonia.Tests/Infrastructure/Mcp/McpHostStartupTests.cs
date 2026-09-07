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

}
