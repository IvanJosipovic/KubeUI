using KubeUI.Desktop;
using Microsoft.Extensions.Hosting;
using Moq;

namespace KubeUI.Avalonia.Tests.Infrastructure;

public sealed class ProgramStartupTests
{
    [Fact]
    public void StartHostAfterAvaloniaSetup_sets_up_Avalonia_before_host_start()
    {
        Mock<IHost> host = new(MockBehavior.Strict);
        List<string> events = [];
        host.Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("host-start"))
            .Returns(Task.CompletedTask);

        Program.StartHostAfterAvaloniaSetup(
            host.Object,
            () => events.Add("avalonia-setup"),
            () => events.Add("avalonia-run"));

        Assert.Equal(["avalonia-setup", "host-start", "avalonia-run"], events);
        host.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
