using k8s.Models;
using KubeUI.Features.Resources.List.Controls;
using KubeUI.Features.Resources.Properties.Controls;
using KubeUI.Controls.Threading;

namespace KubeUI.Features.Resources.Core.v1.Event.Controls;

public sealed class EventLastSeenCell : RelativeTimeCellBase
{
    public EventLastSeenCell(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected override DateTime? ResolveTimestamp(object? dataContext)
        => dataContext is Corev1Event resource
            ? RelativeTimeFormatter.ResolveTimestamp(resource)
            : null;
}
