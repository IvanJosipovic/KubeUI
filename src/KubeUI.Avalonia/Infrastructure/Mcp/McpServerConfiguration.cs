using System.Net;
using System.Net.Sockets;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public static class McpServerConfiguration
{
    public const string Host = "127.0.0.1";
    public const string Path = "/mcp";
    public const int MinimumPort = 1024;
    public const int MaximumPort = 65535;

    /// <summary>
    /// Port value that instructs the operating system to assign an available ephemeral port.
    /// </summary>
    public const int EphemeralPort = 0;

    public static int GetValidatedPort(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return GetValidatedPort(settings.McpServerPort);
    }

    public static int GetValidatedPort(int configuredPort) => configuredPort == EphemeralPort
        ? EphemeralPort
        : Math.Clamp(configuredPort, MinimumPort, MaximumPort);

    /// <summary>
    /// Returns the configured port when it can still be bound on the loopback interface; otherwise
    /// <see cref="EphemeralPort"/> so the operating system assigns a free port and startup never
    /// fails because the configured port is in use or blocked (for example by Windows excluded port ranges).
    /// </summary>
    public static int ResolveAvailablePort(int configuredPort)
    {
        var port = GetValidatedPort(configuredPort);
        return IsPortAvailable(port) ? port : EphemeralPort;
    }

    /// <summary>
    /// Checks whether a TCP listener can bind the given port on the IPv4 loopback interface.
    /// </summary>
    public static bool IsPortAvailable(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public static string GetEndpoint(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return GetEndpoint(GetValidatedPort(settings));
    }

    /// <summary>
    /// Builds the local MCP endpoint URL for the given port, such as the port the server actually bound to.
    /// </summary>
    public static string GetEndpoint(int boundPort) => $"http://{Host}:{boundPort}{Path}";
}
