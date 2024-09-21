using System.Diagnostics.Metrics;
using System.Reflection;
using Scrutor;

namespace KubeUI.Client;

[ServiceDescriptor<Instrumentation>(ServiceLifetime.Singleton)]
public class Instrumentation : IDisposable
{
    public static string MeterName { get; set; } = "kubeui";

    public Counter<long> AppOpened { get; private set; }

    public Counter<long> ViewOpened { get; private set; }

    private readonly Meter _meter;

    public Instrumentation()
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        _meter = new Meter(MeterName, version);

        AppOpened = _meter.CreateCounter<long>(MeterName + "_app_opened", description: "App Opened");

        ViewOpened = _meter.CreateCounter<long>(MeterName + "_view_opened", description: "View Opened");
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
