using Westermo.GraphX.Common.Models;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed class ResourceGraphVertex : VertexBase
{
    public required ResourceNodeViewModel Node { get; init; }

    public required ResourceIdentity Identity { get; init; }
}
