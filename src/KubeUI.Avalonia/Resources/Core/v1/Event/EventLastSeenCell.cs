using k8s.Models;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Core.v1.Event.Controls;

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
