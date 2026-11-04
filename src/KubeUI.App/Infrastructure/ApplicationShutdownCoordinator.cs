using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace KubeUI.App.Infrastructure;

/// <summary>Flushes application telemetry and requests host shutdown once.</summary>
internal sealed class ApplicationShutdownCoordinator(
    IHostApplicationLifetime hostApplicationLifetime,
    IApplicationTelemetryFlusher telemetryFlusher)
{
    private int _shutdownRequested;

    public void RequestShutdown(Action detachKubernetesErrorHandler)
    {
        ArgumentNullException.ThrowIfNull(detachKubernetesErrorHandler);

        if (Interlocked.Exchange(ref _shutdownRequested, 1) == 1)
        {
            return;
        }

        detachKubernetesErrorHandler();
        telemetryFlusher.TryFlush();
        hostApplicationLifetime.StopApplication();
    }
}

/// <summary>Flushes configured OpenTelemetry providers and tolerates late calls after host disposal.</summary>
internal sealed class ApplicationTelemetryFlusher(IServiceProvider services) : IApplicationTelemetryFlusher
{
    private const int FlushTimeoutMilliseconds = 5000;

    public void TryFlush()
    {
        try
        {
            services.GetService<LoggerProvider>()?.ForceFlush(FlushTimeoutMilliseconds);
            services.GetService<MeterProvider>()?.ForceFlush(FlushTimeoutMilliseconds);
            services.GetService<TracerProvider>()?.ForceFlush(FlushTimeoutMilliseconds);
        }
        catch (ObjectDisposedException)
        {
            // Late unobserved task exceptions can arrive after host teardown.
        }
    }
}

/// <summary>Flushes OpenTelemetry providers after reporting an application exception.</summary>
internal interface IApplicationTelemetryFlusher
{
    void TryFlush();
}
