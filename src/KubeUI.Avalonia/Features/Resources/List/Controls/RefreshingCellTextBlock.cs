using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Features.Resources.List.Controls;

public abstract class RefreshingCellTextBlock : TextBlock
{
    private readonly IUiRefreshClock? _refreshClock;
    private IDisposable? _refreshSubscription;

    protected RefreshingCellTextBlock(IUiRefreshClock? refreshClock)
    {
        _refreshClock = refreshClock;
        Name = "CellTextBlock";
        Margin = new Thickness(12, 0, 12, 0);
        VerticalAlignment = VerticalAlignment.Center;
        MaxLines = 1;
    }

    protected abstract string ResolveText(object? dataContext);

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshText();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_refreshClock != null)
        {
            _refreshSubscription = _refreshClock.Subscribe(RefreshText);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
    }

    protected void RefreshText()
    {
        Text = ResolveText(DataContext);
        SetValue(ToolTip.TipProperty, Text);
    }
}
