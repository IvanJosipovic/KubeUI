using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public static class McpServerConfiguration
{
    public const string Host = "127.0.0.1";
    public const string Path = "/mcp";

    public static string GetEndpoint(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return $"http://{Host}:{settings.McpServerPort}{Path}";
    }
}
