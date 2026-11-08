using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace KubeUI.Controls.Layout;

/// <summary>
/// Arranges caller-owned leading, middle, and trailing content in a compact toolbar.
/// Trailing content scrolls horizontally and moves below the other slots when fewer than
/// 32 pixels remain. Middle content receives a bounded width so it can adapt to available space.
/// No application resources, commands, or domain models are required.
/// </summary>
public sealed class CompactToolbar : Panel
{
    private const string CompactControlClass = "CompactToolbarControl";

    /// <summary>Identifies <see cref="Leading"/>.</summary>
    public static readonly DirectProperty<CompactToolbar, Control?> LeadingProperty =
        AvaloniaProperty.RegisterDirect<CompactToolbar, Control?>(nameof(Leading), x => x.Leading, (x, value) => x.Leading = value);

    /// <summary>Identifies <see cref="Middle"/>.</summary>
    public static readonly DirectProperty<CompactToolbar, Control?> MiddleProperty =
        AvaloniaProperty.RegisterDirect<CompactToolbar, Control?>(nameof(Middle), x => x.Middle, (x, value) => x.Middle = value);

    /// <summary>Identifies <see cref="Trailing"/>.</summary>
    public static readonly DirectProperty<CompactToolbar, Control?> TrailingProperty =
        AvaloniaProperty.RegisterDirect<CompactToolbar, Control?>(nameof(Trailing), x => x.Trailing, (x, value) => x.Trailing = value);

    private readonly ScrollViewer _trailingScroll;
    private Control? _leading;
    private Control? _middle;
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

        Styles.Add(new Style<Control>(style => style.Class(CompactControlClass))
            .Setter(MarginProperty, new Thickness(0))
            .Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
        Styles.Add(new Style<TemplatedControl>(style => style.Class(CompactControlClass))
            .Setter(HeightProperty, 32d)
            .Setter(MinHeightProperty, 32d));
        Styles.Add(new Style<ComboBox>(style => style.Class(CompactControlClass))
            .Setter(HeightProperty, 32d)
            .Setter(MinHeightProperty, 32d)
            .Setter(ComboBox.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        Styles.Add(new Style<TextBox>(style => style.Class(CompactControlClass))
            .Setter(HeightProperty, 32d)
            .Setter(MinHeightProperty, 32d)
            .Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        Styles.Add(new Style<Button>(style => style.Class(CompactControlClass))
            .Setter(HeightProperty, 32d)
            .Setter(MinHeightProperty, 32d)
            .Setter(MinWidthProperty, 32d));
        Styles.Add(new Style<ToggleButton>(style => style.Class(CompactControlClass))
            .Setter(HeightProperty, 32d)
            .Setter(MinHeightProperty, 32d)
            .Setter(MinWidthProperty, 32d));
        Styles.Add(new Style<ContentControl>(style => style.Class(CompactControlClass))
            .Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center));
    }

    /// <summary>
    /// Creates a standard horizontal group with four-pixel spacing. Toolbar-scoped styles
    /// center its controls, remove margins, and set compact minimum sizes.
    /// </summary>
    /// <param name="controls">Controls to place in the group, in display order.</param>
    public static StackPanel Group(params Control[] controls) => Group(4, controls);

    /// <summary>Creates a horizontal group with caller-selected spacing and toolbar-scoped defaults.</summary>
    /// <param name="spacing">Spacing between controls.</param>
    /// <param name="controls">Controls to place in the group, in display order.</param>
    public static StackPanel Group(double spacing, params Control[] controls)
    {
        ArgumentNullException.ThrowIfNull(controls);
        var group = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = spacing,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var control in controls)
        {
            ArgumentNullException.ThrowIfNull(control);
            MarkCompactControl(control);
            group.Children.Add(control);
        }

        return group;
    }

    /// <summary>Gets or sets the leading action/content, inset two pixels from the left.</summary>
    public Control? Leading
    {
        get => _leading;
        set
        {
            var previous = _leading;
            if (SetAndRaise(LeadingProperty, ref _leading, value))
            {
                if (value is not null)
                    MarkCompactControl(value);
                ReplaceSlot(previous, value, 0);
            }
        }
    }

    /// <summary>Gets or sets optional middle content, inset two pixels after the leading slot.</summary>
    public Control? Middle
    {
        get => _middle;
        set
        {
            var previous = _middle;
            if (SetAndRaise(MiddleProperty, ref _middle, value))
            {
                if (value is not null)
                    MarkCompactControl(value);
                ReplaceSlot(previous, value, _leading is null ? 0 : 1);
            }
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
                if (value is not null)
                    MarkCompactControl(value);
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
        var middleWidth = MeasureSlot(_middle, Math.Max(0, width - leadingWidth), availableSize.Height);
        var leftWidth = leadingWidth + middleWidth;
        _firstRowHeight = Math.Max(VisibleHeight(_leading), VisibleHeight(_middle));

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
        x += ArrangeSlot(_middle, x, rowHeight);
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

    private static void MarkCompactControl(Control control)
    {
        if (!control.Classes.Contains(CompactControlClass))
            control.Classes.Add(CompactControlClass);
    }
}
