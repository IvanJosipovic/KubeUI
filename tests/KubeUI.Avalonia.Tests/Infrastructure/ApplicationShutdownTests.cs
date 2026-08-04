using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure;

public sealed class ApplicationShutdownTests
{
    [Fact]
    public void host_stopping_shuts_down_the_desktop_lifetime()
    {
        using var stopping = new CancellationTokenSource();
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        hostLifetime.SetupGet(x => x.ApplicationStopping).Returns(stopping.Token);
        using var services = new ServiceCollection()
            .AddSingleton(hostLifetime.Object)
            .BuildServiceProvider();
        var shutdownRequested = false;

        Desktop.Program.RegisterAvaloniaShutdown(services, () =>
        {
            shutdownRequested = true;
        });

        stopping.Cancel();

        shutdownRequested.ShouldBeTrue();
    }

    [Fact]
    public async Task avalonia_shutdown_requests_real_host_stop()
    {
        using var host = Desktop.Program.CreateHostBuilder([], includeOptionalServices: false).Build();
        await host.StartAsync();

        var hostStopping = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = host.Services
            .GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStopping
            .Register(hostStopping.SetResult);

        Desktop.Program.CreateAppBuilder(host.Services).SetupWithoutStarting();
        var app = Application.Current.ShouldBeOfType<App>();
        app.GracefulShutdown();

        await hostStopping.Task.WaitAsync(TimeSpan.FromSeconds(5));
        hostStopping.Task.IsCompletedSuccessfully.ShouldBeTrue();

        using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await host.StopAsync(shutdownTimeout.Token);
    }
}
