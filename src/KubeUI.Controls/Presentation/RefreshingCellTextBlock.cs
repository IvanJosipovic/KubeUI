using KubeUI.Controls.Threading;

namespace KubeUI.Controls.Presentation;

/// <summary>
/// Displays cell text and a matching tooltip, refreshing on data-context changes and,
/// when a clock is supplied, on clock ticks while attached to the visual tree.
/// </summary>
public abstract class RefreshingCellTextBlock : TextBlock
{
    private readonly IUiRefreshClock? _refreshClock;
    private IDisposable? _refreshSubscription;

    /// <summary>
    /// Initializes a single-line, vertically centered cell named <c>CellTextBlock</c>.
    /// </summary>
    /// <param name="refreshClock">
    /// The clock to subscribe to while attached, or <see langword="null"/> to refresh only
    /// on data-context changes and explicit <see cref="RefreshText"/> calls.
    /// </param>
    protected RefreshingCellTextBlock(IUiRefreshClock? refreshClock)
    {
        _refreshClock = refreshClock;
        Name = "CellTextBlock";
        VerticalAlignment = VerticalAlignment.Center;
        MaxLines = 1;
    }

    /// <summary>Resolves the displayed text for the current data context.</summary>
    /// <param name="dataContext">The current data context, which may be null.</param>
    /// <returns>The text to display in the cell and its tooltip.</returns>
    protected abstract string ResolveText(object? dataContext);

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshText();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_refreshClock != null)
        {
            _refreshSubscription = _refreshClock.Subscribe(RefreshText);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
    }

    /// <summary>Updates the cell text and tooltip using the current data context.</summary>
    protected void RefreshText()
    {
        Text = ResolveText(DataContext);
        SetValue(ToolTip.TipProperty, Text);
    }
}
