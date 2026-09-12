using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public static class McpServerConfiguration
{
    public const string Host = "127.0.0.1";
    public const string Path = "/mcp";

    /// <summary>Builds the local HTTP endpoint used by the embedded MCP server.</summary>
    /// <param name="settings">Settings containing the configured MCP server port.</param>
    /// <returns>Loopback MCP endpoint using configured port and <see cref="Path"/>.</returns>
    public static string GetEndpoint(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return $"http://{Host}:{settings.McpServerPort}{Path}";
    }

    /// <summary>Builds a local HTTP endpoint for the specified MCP server port.</summary>
    /// <param name="port">TCP port used by the MCP server.</param>
    /// <returns>The loopback MCP endpoint.</returns>
    public static string GetEndpoint(int port) => $"http://{Host}:{port}{Path}";
}
