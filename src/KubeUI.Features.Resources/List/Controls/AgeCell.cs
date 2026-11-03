using k8s;
using k8s.Models;
using KubeUI.Controls.Threading;

namespace KubeUI.Features.Resources.List.Controls;

public sealed class AgeCell : RelativeTimeCellBase
{
    public AgeCell(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected override DateTime? ResolveTimestamp(object? dataContext)
        => dataContext is IKubernetesObject<V1ObjectMeta> resource
            ? resource.Metadata.CreationTimestamp
            : null;
}
