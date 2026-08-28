using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public static class McpServerConfiguration
{
    public const string Host = "127.0.0.1";
    public const string Path = "/mcp";
    public const int MinimumPort = 1024;
    public const int MaximumPort = 65535;

    public static int GetValidatedPort(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return GetValidatedPort(settings.McpServerPort);
    }

    public static int GetValidatedPort(int configuredPort) => Math.Clamp(configuredPort, MinimumPort, MaximumPort);

    public static string GetEndpoint(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return GetEndpoint(settings, boundPort: null);
    }

    public static string GetEndpoint(AppSettings settings, int? boundPort)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return $"http://{Host}:{(boundPort ?? GetValidatedPort(settings))}{Path}";
    }
}
