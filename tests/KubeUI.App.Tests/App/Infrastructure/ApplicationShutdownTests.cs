using System.Diagnostics;
using Avalonia.Threading;
using Dock.Model.Core;
using KubeUI.App.Infrastructure;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.Features.Clusters.Error;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace KubeUI.App.Tests.App.Infrastructure;

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

        KubeUI.Desktop.Program.RegisterAvaloniaShutdown(services, () =>
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
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);

        using var app = new TestApp(services);
        app.GracefulShutdown();

        app.TelemetryWasFlushed.ShouldBeTrue();
        hostLifetime.Verify(x => x.StopApplication(), Times.Once);
    }

    [Fact]
    public void repeated_shutdown_requests_flush_and_stop_host_once()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        using var services = CreateServices(hostLifetime.Object, loggerProvider);
        using var app = new TestApp(services);

        app.GracefulShutdown();
        app.GracefulShutdown();

        services.GetRequiredService<RecordingTelemetryFlusher>().FlushCount.ShouldBe(1);
        hostLifetime.Verify(x => x.StopApplication(), Times.Once);
    }

    [Fact]
    public void disposing_application_detaches_all_process_error_handlers()
    {
        var hostLifetime = new Mock<IHostApplicationLifetime>();
        using var loggerProvider = new CapturingLoggerProvider();
        var eventSource = new FakeApplicationEventSource();
        using var services = CreateServices(hostLifetime.Object, loggerProvider, eventSource);
        var app = new TestApp(services);

        eventSource.SubscriberCount.ShouldBe(4);

        app.Dispose();

        eventSource.SubscriberCount.ShouldBe(0);
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
        var flusher = new ApplicationTelemetryFlusher(new DisposedServiceProvider());

        Should.NotThrow(flusher.TryFlush);
    }

    private static ServiceProvider CreateServices(
        IHostApplicationLifetime hostLifetime,
        ILoggerProvider loggerProvider,
        IApplicationEventSource? eventSource = null)
    {
        var services = new ServiceCollection()
            .AddLogging(builder => builder.AddProvider(loggerProvider))
            .AddSingleton(hostLifetime)
            .AddSingleton<Instrumentation>()
            .AddSingleton<ViewLocator>()
            .AddSingleton(eventSource ?? new FakeApplicationEventSource())
            .AddSingleton<RecordingTelemetryFlusher>()
            .AddSingleton<IApplicationTelemetryFlusher>(sp => sp.GetRequiredService<RecordingTelemetryFlusher>())
            .AddSingleton<ApplicationShutdownCoordinator>()
            .AddSingleton(new Mock<IFactory>().Object)
            .AddTransient<ClusterErrorViewModel>()
            .AddTransient<Func<ClusterErrorViewModel>>(sp => () => sp.GetRequiredService<ClusterErrorViewModel>())
            .AddSingleton<IClusterErrorDocumentPresenter>(sp => new ClusterErrorDocumentPresenter(
                () => sp.GetRequiredService<IFactory>(),
                sp.GetRequiredService<Func<ClusterErrorViewModel>>()))
            .AddSingleton<ApplicationErrorHandler>();

        return services.BuildServiceProvider();
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

    private sealed class TestApp(IServiceProvider services) : KubeUI.App.App(services), IDisposable
    {
        public bool TelemetryWasFlushed => services.GetRequiredService<RecordingTelemetryFlusher>().FlushCount > 0;

        public void Dispose() => DisposeApplication();
    }

    private sealed class RecordingTelemetryFlusher : IApplicationTelemetryFlusher
    {
        private int _flushCount;

        public int FlushCount => Volatile.Read(ref _flushCount);

        public void TryFlush() => Interlocked.Increment(ref _flushCount);
    }

    private sealed class DisposedServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => throw new ObjectDisposedException(nameof(DisposedServiceProvider));
    }

    private sealed class FakeApplicationEventSource : IApplicationEventSource
    {
        private UnhandledExceptionEventHandler? _currentDomainUnhandledException;
        private DispatcherUnhandledExceptionEventHandler? _dispatcherUnhandledException;
        private EventHandler<UnobservedTaskExceptionEventArgs>? _unobservedTaskException;
        private EventHandler<DataReceivedEventArgs>? _kubernetesExecStdError;

        public int SubscriberCount =>
            (_currentDomainUnhandledException?.GetInvocationList().Length ?? 0)
            + (_dispatcherUnhandledException?.GetInvocationList().Length ?? 0)
            + (_unobservedTaskException?.GetInvocationList().Length ?? 0)
            + (_kubernetesExecStdError?.GetInvocationList().Length ?? 0);

        public event UnhandledExceptionEventHandler? CurrentDomainUnhandledException
        {
            add => _currentDomainUnhandledException += value;
            remove => _currentDomainUnhandledException -= value;
        }

        public event DispatcherUnhandledExceptionEventHandler? DispatcherUnhandledException
        {
            add => _dispatcherUnhandledException += value;
            remove => _dispatcherUnhandledException -= value;
        }

        public event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException
        {
            add => _unobservedTaskException += value;
            remove => _unobservedTaskException -= value;
        }

        public event EventHandler<DataReceivedEventArgs>? KubernetesExecStdError
        {
            add => _kubernetesExecStdError += value;
            remove => _kubernetesExecStdError -= value;
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
