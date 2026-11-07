using Avalonia.Controls.Primitives;

namespace KubeUI.Controls.Presentation;

/// <summary>An initially expanded section with compact header, chevron, and content spacing.</summary>
/// <remarks>
/// Uses the host's <see cref="Expander"/> theme without requiring application-specific styles.
/// Compact defaults are local resources named <c>ExpanderMinHeight</c>,
/// <c>ExpanderHeaderPadding</c>, <c>ExpanderChevronMargin</c>,
/// <c>ExpanderChevronButtonSize</c>, and <c>ExpanderContentPadding</c>.
/// Callers can replace these resources to customize an individual section.
/// Header, content, and expansion state use the inherited <see cref="Expander"/> API.
/// </remarks>
public partial class ExpandableSection : Expander, IDeclarativeViewBase
{
    /// <summary>Initializes a new expandable section with default compact styling.</summary>
    public ExpandableSection()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Top;
        IsExpanded = true;
        Resources.Add("ExpanderMinHeight", 28d);
        Resources.Add("ExpanderHeaderPadding", new Thickness(10, 0, 0, 0));
        Resources.Add("ExpanderChevronMargin", new Thickness(8, 0, 4, 0));
        Resources.Add("ExpanderChevronButtonSize", 24d);
        Resources.Add("ExpanderContentPadding", new Thickness(5, 0, 0, 0));

#if DEBUG
        if (Design.IsDesignMode)
        {
            Header = "Header";
            Content = new StackPanel();
            var panel = (Content as StackPanel);

            for (var i = 0; i < 5; i++)
            {
                panel.Children.Add(new PropertyItem() { Key = "Name" + i, Value = "myValue" + i });
            }

            IsExpanded = true;
        }
#endif
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Expander);
}
