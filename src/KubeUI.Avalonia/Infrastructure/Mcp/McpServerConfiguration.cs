using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public static class McpServerConfiguration
{
    public const string Host = "127.0.0.1";
    public const string Path = "/mcp";
    public const int MinimumPort = 1024;
    public const int MaximumPort = 65535;

    /// <summary>
    /// Port value that instructs the server to bind to an ephemeral port assigned by the operating system.
    /// </summary>
    public const int DynamicPort = 0;

    public static int GetValidatedPort(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return GetValidatedPort(settings.McpServerPort);
    }

    public static int GetValidatedPort(int configuredPort) => configuredPort == DynamicPort
        ? DynamicPort
        : Math.Clamp(configuredPort, MinimumPort, MaximumPort);

    public static string GetEndpoint(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return GetEndpoint(settings, boundPort: null);
    }

    /// <summary>
    /// Builds the local MCP endpoint, preferring the port the server actually bound to over the configured one.
    /// </summary>
    public static string GetEndpoint(AppSettings settings, int? boundPort)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return $"http://{Host}:{boundPort ?? GetValidatedPort(settings)}{Path}";
    }
}
