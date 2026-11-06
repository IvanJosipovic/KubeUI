namespace KubeUI.Features.AI.Mcp;

public static class McpServerConfiguration
{
    /// <summary>The IPv4 loopback host used by the embedded MCP server.</summary>
    public const string Host = "127.0.0.1";

    /// <summary>The loopback hostname accepted by the embedded MCP server.</summary>
    public const string Localhost = "localhost";

    public const string Path = "/mcp";

    /// <summary>Builds a local HTTP endpoint for the specified MCP server port.</summary>
    /// <param name="port">TCP port used by the MCP server.</param>
    /// <returns>The loopback MCP endpoint.</returns>
    public static string GetEndpoint(int port) => $"http://{Host}:{port}{Path}";
}
