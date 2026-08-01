using k8s;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Features.Resources.List.Controls;

public sealed partial class AgeCell : UserControl
{
    private readonly IUiRefreshClock _refreshClock;
    private IDisposable? _refreshSubscription;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public AgeCell(IUiRefreshClock refreshClock)
    {
        _refreshClock = refreshClock;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        Content = CreateCellTextBlock();

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Namespace()
            {
                Metadata = new()
                {
                    CreationTimestamp = DateTime.UtcNow,
                }
            };
        }
#endif
    }

    private TextBlock CreateCellTextBlock()
    {
        return new TextBlock()
            .Name("CellTextBlock")
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(this, x => x.PrettyString)
            .ToolTip_Tip(this, x => x.PrettyString);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetPrettyString();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _refreshSubscription = _refreshClock.Subscribe(SetPrettyString);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
    }

    private void SetPrettyString()
    {
        if (DataContext is IKubernetesObject<V1ObjectMeta> obj)
        {
            if (obj.Metadata.CreationTimestamp.HasValue)
            {
                var _date = obj.Metadata.CreationTimestamp.Value;

                var d = DateTime.UtcNow - _date;
                if (d.TotalMilliseconds <= 0)
                    PrettyString = "0ms";
                else if (d.TotalDays >= 365)
                    PrettyString = $"{(d.TotalDays / 365):N0}y";
                else if (d.TotalDays >= 1)
                    PrettyString = $"{d.TotalDays:N0}d";
                else if (d.TotalHours >= 1)
                    PrettyString = $"{d.TotalHours:N0}h";
                else if (d.TotalMinutes >= 1)
                    PrettyString = $"{d.TotalMinutes:N0}m{d.Seconds:N0}s";
                else if (d.TotalSeconds >= 1)
                    PrettyString = $"{d.TotalSeconds:N0}s";
                else
                    PrettyString = $"{d.TotalMilliseconds:N0}ms";
            }
            else
            {
                PrettyString = string.Empty;
            }
        }
    }
}

