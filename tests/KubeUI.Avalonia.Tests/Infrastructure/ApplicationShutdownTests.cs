using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Logs;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
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
    public void avalonia_shutdown_requests_host_stop()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(hostLifetime.Object)
            .AddSingleton<Instrumentation>()
            .AddSingleton<ViewLocator>()
            .BuildServiceProvider();

        var app = new TestApp(services);
        app.GracefulShutdown();

        app.TelemetryWasFlushed.ShouldBeTrue();
        hostLifetime.Verify(x => x.StopApplication(), Times.Once);
    }

    [Fact]
    public void unhandled_runtime_exception_flushes_telemetry()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var processor = new RecordingLogProcessor();
        var serviceCollection = new ServiceCollection()
            .AddLogging();
        serviceCollection.AddOpenTelemetry()
            .WithLogging(logging => logging.AddProcessor(processor));
        using var services = serviceCollection
            .AddSingleton(hostLifetime.Object)
            .AddSingleton<Instrumentation>()
            .AddSingleton<ViewLocator>()
            .BuildServiceProvider();
        var app = new TestApp(services);

        app.RecordUnhandledException(new InvalidOperationException("runtime failure"), isTerminating: false);

        processor.Records.ShouldContain(record => record.LogLevel == LogLevel.Critical);
        hostLifetime.Verify(x => x.StopApplication(), Times.Never);
    }

    private sealed class TestApp(IServiceProvider services) : App(services)
    {
        public bool TelemetryWasFlushed { get; private set; }

        protected override void FlushTelemetry()
        {
            TelemetryWasFlushed = true;
        }
    }

    private sealed class RecordingLogProcessor : BaseProcessor<LogRecord>
    {
        public List<LogRecord> Records { get; } = [];

        public override void OnEnd(LogRecord data)
        {
            Records.Add(data);
        }
    }

}
