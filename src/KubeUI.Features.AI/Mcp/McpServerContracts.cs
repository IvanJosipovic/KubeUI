using System.ComponentModel;

namespace KubeUI.Features.AI.Mcp;

/// <summary>Exposes the port currently bound by the local MCP server.</summary>
public interface IMcpServerState : INotifyPropertyChanged
{
    /// <summary>Gets the bound port, or <see langword="null"/> when the server did not bind.</summary>
    int? BoundPort { get; }
}

/// <summary>Limits applied to the local MCP endpoint.</summary>
internal static class McpServerRateLimits
{
    public const string ClientRateLimitPolicyName = "KubeUI.Mcp.Client";
    public const int MaximumConcurrentRequests = 16;
    public const int MaximumRequestsPerMinute = 120;
    public const int MaximumRequestsPerMinutePerAddress = 600;
}
