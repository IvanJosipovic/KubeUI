using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        using var app = new TestApp(services);
        app.GracefulShutdown();

        app.TelemetryWasFlushed.ShouldBeTrue();
        hostLifetime.Verify(x => x.StopApplication(), Times.Once);
    }

    [Fact]
    public void unhandled_runtime_exception_flushes_telemetry()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new TestApp(services);
        var exception = CreateException("runtime failure");

        app.RecordUnhandledException(exception, isTerminating: false);

        loggerProvider.Records.ShouldContain(record =>
            record.LogLevel == LogLevel.Critical &&
            ReferenceEquals(record.Exception, exception) &&
            record.Message.Contains("Unhandled exception", StringComparison.Ordinal) &&
            exception.ToString().Contains("runtime failure", StringComparison.Ordinal));
        hostLifetime.Verify(x => x.StopApplication(), Times.Never);
    }

    [Fact]
    public void ui_thread_exception_flushes_telemetry_without_stopping_host()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new TestApp(services);
        var exception = CreateException("ui failure");

        app.HandleUiThreadException(exception);

        app.TelemetryWasFlushed.ShouldBeTrue();
        loggerProvider.Records.ShouldContain(record =>
            record.LogLevel == LogLevel.Critical &&
            ReferenceEquals(record.Exception, exception) &&
            exception.ToString().Contains("ui failure", StringComparison.Ordinal));
        hostLifetime.Verify(x => x.StopApplication(), Times.Never);
    }

    [Fact]
    public void terminating_exception_flushes_telemetry_and_stops_host()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new TestApp(services);
        var exception = CreateException("fatal failure");

        app.RecordUnhandledException(exception, isTerminating: true);

        loggerProvider.Records.ShouldContain(record =>
            record.LogLevel == LogLevel.Critical &&
            ReferenceEquals(record.Exception, exception) &&
            exception.ToString().Contains("fatal failure", StringComparison.Ordinal));
        hostLifetime.Verify(x => x.StopApplication(), Times.Once);
    }

    [Fact]
    public void process_unhandled_exception_logs_critical_details()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new TestApp(services);
        var exception = CreateException("process crash");

        app.CurrentDomain_UnhandledException(this, new UnhandledExceptionEventArgs(exception, isTerminating: false));

        loggerProvider.Records.ShouldContain(record =>
            record.LogLevel == LogLevel.Critical &&
            ReferenceEquals(record.Exception, exception) &&
            exception.ToString().Contains("process crash", StringComparison.Ordinal));
    }

    [Fact]
    public void unobserved_task_exception_logs_critical_details_and_is_observed()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new TestApp(services);
        var exception = CreateException("background task crash");
        var eventArgs = new UnobservedTaskExceptionEventArgs(new AggregateException(exception));

        app.TaskScheduler_UnobservedTaskException(this, eventArgs);

        eventArgs.Observed.ShouldBeTrue();
        loggerProvider.Records.Any(record =>
            record.LogLevel == LogLevel.Critical &&
            record.Exception is AggregateException aggregate &&
            aggregate.InnerExceptions.Contains(exception) &&
            aggregate.ToString().Contains("background task crash", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [Fact]
    public void late_unhandled_exception_does_not_throw_after_services_are_disposed()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new DisposedTelemetryApp(services);
        services.Dispose();

        Should.NotThrow(() => app.RecordUnhandledException(
            new InvalidOperationException("late task failure"),
            isTerminating: false));
    }

    private static ServiceProvider CreateServices(
        IHostApplicationLifetime hostLifetime,
        ILoggerProvider loggerProvider)
    {
        return new ServiceCollection()
            .AddLogging(builder => builder.AddProvider(loggerProvider))
            .AddSingleton(hostLifetime)
            .AddSingleton<Instrumentation>()
            .AddSingleton<ViewLocator>()
            .BuildServiceProvider();
    }

    private static InvalidOperationException CreateException(string message)
    {
        try
        {
            throw new InvalidOperationException(message);
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    private sealed class TestApp(IServiceProvider services) : App(services), IDisposable
    {
        public bool TelemetryWasFlushed { get; private set; }

        public void Dispose() => DisposeApplication();

        protected override void FlushTelemetry()
        {
            TelemetryWasFlushed = true;
        }
    }

    private sealed class DisposedTelemetryApp(IServiceProvider services) : App(services), IDisposable
    {
        public void Dispose() => DisposeApplication();

        protected override void FlushTelemetry()
        {
            throw new ObjectDisposedException(nameof(IServiceProvider));
        }
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<CapturedLog> Records { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Records);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(List<CapturedLog> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            records.Add(new CapturedLog(logLevel, exception, formatter(state, exception)));
        }
    }

    private sealed record CapturedLog(LogLevel LogLevel, Exception? Exception, string Message);
}
