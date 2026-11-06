using System.Diagnostics;

namespace KubeUI.Features.Clusters.Workspace;

/// <summary>Provides the activity source used by cluster workspace operations.</summary>
public interface IClusterInstrumentation
{
    ActivitySource Source { get; }
}
