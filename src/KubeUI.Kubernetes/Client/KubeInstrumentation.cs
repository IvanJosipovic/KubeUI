using System.Diagnostics;
using System.Reflection;

namespace KubeUI.Kubernetes.Client;

public class KubeInstrumentation : IDisposable
{
    public static ActivitySource Source { get; private set; }

    public static string MeterName { get; set; } = "kubeui";

    public static string SourceName => "com.KubeUI.Kubernetes";

    static KubeInstrumentation()
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        Source = new ActivitySource(SourceName, version);
    }

    public KubeInstrumentation()
    {

    }

    public void Dispose()
    {
        Source.Dispose();
    }
}
