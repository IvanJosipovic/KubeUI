using k8s.Models;
using KubeUI.Controls.Threading;
using KubeUI.Features.Resources.List.Controls;
using KubeUI.Features.Resources.Properties.Controls;

#pragma warning disable CA1716 // Public namespace is an API contract; renaming would be a breaking change.
namespace KubeUI.Features.Resources.Core.v1.Event.Controls;
#pragma warning restore CA1716

public sealed class EventLastSeenCell : RelativeTimeCellBase
{
    public EventLastSeenCell(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected override DateTime? ResolveTimestamp(object? dataContext)
        => dataContext is Corev1Event resource
            ? KubernetesEventTimestampResolver.Resolve(resource)
            : null;
}
