using Microsoft.Extensions.Options;

namespace KubeUI.Kubernetes;

/// <summary>Options controlling cluster authorization behavior.</summary>
public sealed class ClusterAuthorizationOptions
{
    /// <summary>
    /// Time window to coalesce multiple forbidden responses into a single permission cache invalidation.
    /// Default is 5 seconds.
    /// </summary>
    public TimeSpan ForbiddenCoalesceWindow { get; set; } = TimeSpan.FromSeconds(5);
}