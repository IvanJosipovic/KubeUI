using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace KubeUI.Controls.Layout;

/// <summary>
/// Arranges caller-owned leading, count/status, and trailing controls in a compact toolbar.
/// Trailing content scrolls horizontally and moves below the other slots when fewer than
/// 32 pixels remain. Count content receives a bounded width so caller-supplied text can wrap.
/// No application resources, commands, or domain models are required.
/// </summary>
public sealed class CompactToolbar : Panel
{
    /// <summary>Identifies <see cref="Leading"/>.</summary>
    public static readonly DirectProperty<CompactToolbar, Control?> LeadingProperty =
        AvaloniaProperty.RegisterDirect<CompactToolbar, Control?>(nameof(Leading), x => x.Leading, (x, value) => x.Leading = value);

    /// <summary>Identifies <see cref="Count"/>.</summary>
    public static readonly DirectProperty<CompactToolbar, Control?> CountProperty =
        AvaloniaProperty.RegisterDirect<CompactToolbar, Control?>(nameof(Count), x => x.Count, (x, value) => x.Count = value);

    /// <summary>Identifies <see cref="Trailing"/>.</summary>
    public static readonly DirectProperty<CompactToolbar, Control?> TrailingProperty =
        AvaloniaProperty.RegisterDirect<CompactToolbar, Control?>(nameof(Trailing), x => x.Trailing, (x, value) => x.Trailing = value);

    private readonly ScrollViewer _trailingScroll;
    private Control? _leading;
    private Control? _count;
    private Control? _trailing;
    private bool _wrapTrailing;
    private double _firstRowHeight;

    /// <summary>Creates an empty toolbar with the shared compact sizing and spacing.</summary>
    public CompactToolbar()
    {
        MinHeight = 32;
        Margin = new Thickness(0, 2);
        _trailingScroll = new ScrollViewer
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Right,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        Children.Add(_trailingScroll);
    }

    /// <summary>Gets or sets the leading action/content, inset two pixels from the left.</summary>
    public Control? Leading
    {
        get => _leading;
        set
        {
            var previous = _leading;
            if (SetAndRaise(LeadingProperty, ref _leading, value))
                ReplaceSlot(previous, value, 0);
        }
    }

    /// <summary>Gets or sets the count/status content, inset two pixels after the leading slot.</summary>
    public Control? Count
    {
        get => _count;
        set
        {
            var previous = _count;
            if (SetAndRaise(CountProperty, ref _count, value))
                ReplaceSlot(previous, value, _leading is null ? 0 : 1);
        }
    }

    /// <summary>
    /// Gets or sets right-aligned, horizontally scrollable content. Callers own commands,
    /// accessible names, visibility, and any wrapping or maximum widths within this slot.
    /// </summary>
    public Control? Trailing
    {
        get => _trailing;
        set
        {
            if (SetAndRaise(TrailingProperty, ref _trailing, value))
            {
                _trailingScroll.Content = value;
                InvalidateMeasure();
            }
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width;
        var leadingWidth = MeasureSlot(_leading, width, availableSize.Height);
        var countWidth = MeasureSlot(_count, Math.Max(0, width - leadingWidth), availableSize.Height);
        var leftWidth = leadingWidth + countWidth;
        _firstRowHeight = Math.Max(VisibleHeight(_leading), VisibleHeight(_count));

        _trailingScroll.Measure(new Size(double.PositiveInfinity, availableSize.Height));
        var trailingWidth = _trailingScroll.DesiredSize.Width;
        _wrapTrailing = width - leftWidth < Math.Min(32, trailingWidth);
        var trailingAvailable = _wrapTrailing ? width : Math.Max(0, width - leftWidth);
        _trailingScroll.Measure(new Size(trailingAvailable, availableSize.Height));

        var height = _wrapTrailing
            ? _firstRowHeight + _trailingScroll.DesiredSize.Height
            : Math.Max(_firstRowHeight, _trailingScroll.DesiredSize.Height);
        return new Size(Math.Min(width, leftWidth + trailingWidth), height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var rowHeight = _wrapTrailing ? _firstRowHeight : finalSize.Height;
        var x = ArrangeSlot(_leading, 0, rowHeight);
        x += ArrangeSlot(_count, x, rowHeight);
        _trailingScroll.Arrange(_wrapTrailing
            ? new Rect(0, rowHeight, finalSize.Width, Math.Max(0, finalSize.Height - rowHeight))
            : new Rect(x, 0, Math.Max(0, finalSize.Width - x), finalSize.Height));
        return finalSize;
    }

    private void ReplaceSlot(Control? previous, Control? value, int index)
    {
        if (previous is not null)
            Children.Remove(previous);
        if (value is not null)
            Children.Insert(index, value);
        InvalidateMeasure();
    }

    private static double MeasureSlot(Control? control, double width, double height)
    {
        if (control is null)
            return 0;
        control.Measure(new Size(Math.Max(0, width - 2), height));
        return control.IsVisible ? control.DesiredSize.Width + 2 : 0;
    }

    private static double ArrangeSlot(Control? control, double x, double height)
    {
        if (control is null || !control.IsVisible)
            return 0;
        control.Arrange(new Rect(x + 2, 0, control.DesiredSize.Width, height));
        return control.DesiredSize.Width + 2;
    }

    private static double VisibleHeight(Control? control)
        => control is { IsVisible: true } ? control.DesiredSize.Height : 0;
}
