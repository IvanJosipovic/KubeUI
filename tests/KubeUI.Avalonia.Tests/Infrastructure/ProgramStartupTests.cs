using KubeUI.Desktop;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.Hosting;
using Moq;

namespace KubeUI.Avalonia.Tests.Infrastructure;

public sealed class ProgramStartupTests
{
    [Fact]
    public void CreateHostBuilder_enables_static_yaml_metadata()
    {
        var previousValue = KubernetesYaml.UseStaticContext;
        try
        {
            KubernetesYaml.UseStaticContext = false;
            using var host = Program.CreateHostBuilder([], includeOptionalServices: false).Build();

            Assert.True(KubernetesYaml.UseStaticContext);
        }
        finally
        {
            KubernetesYaml.UseStaticContext = previousValue;
        }
    }

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
