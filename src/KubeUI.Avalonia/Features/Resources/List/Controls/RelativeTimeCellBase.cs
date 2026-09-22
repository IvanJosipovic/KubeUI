using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Features.Resources.List.Controls;

public abstract class RelativeTimeCellBase : RefreshingCellTextBlock
{
    protected RelativeTimeCellBase(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected sealed override string ResolveText(object? dataContext)
    {
        var timestamp = ResolveTimestamp(dataContext);
        return timestamp.HasValue
            ? RelativeTimeFormatter.FormatPrettyAge(timestamp.Value, DateTime.UtcNow)
            : string.Empty;
    }

    protected abstract DateTime? ResolveTimestamp(object? dataContext);
}
