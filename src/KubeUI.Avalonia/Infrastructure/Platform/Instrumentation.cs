using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace KubeUI.Avalonia.Infrastructure.Platform;

public class Instrumentation : IDisposable
{
    public ActivitySource Source { get; private set; }

    public Meter Meter { get; private set; }

    public static string MeterName { get; set; } = "kubeui";

    public static string SourceName => "com.KubeUI.Avalonia";

    public Counter<long> AppOpened { get; }

    public Counter<long> ViewOpened { get; }

    public Instrumentation()
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        Source = new ActivitySource(SourceName, version);

        Meter = new Meter(MeterName, version);

        AppOpened = Meter.CreateCounter<long>(MeterName + "_app_opened", description: "App Opened");

        ViewOpened = Meter.CreateCounter<long>(MeterName + "_view_opened", description: "View Opened");
    }

    public void Dispose()
    {
        Meter.Dispose();
        Source.Dispose();
    }
}

