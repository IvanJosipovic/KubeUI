using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace KubeUI.Views;

public partial class ExpandableSection : ViewBase
{
    public static readonly DirectProperty<ExpandableSection, bool> IsExpandedProperty =
    AvaloniaProperty.RegisterDirect<ExpandableSection, bool>(
    nameof(IsExpanded),
    o => o.IsExpanded,
    (o, v) => o.IsExpanded = v);

    private bool _isExpanded;

    public bool IsExpanded
    {
        get { return _isExpanded; }
        set { SetAndRaise(IsExpandedProperty, ref _isExpanded, value); }
    }

    public static readonly DirectProperty<ExpandableSection, string> TextProperty =
        AvaloniaProperty.RegisterDirect<ExpandableSection, string>(
        nameof(Text),
        o => o.Text,
        (o, v) => o.Text = v);

    private string _text = string.Empty;

    public string Text
    {
        get { return _text; }
        set { SetAndRaise(TextProperty, ref _text, value); }
    }

    public static readonly DirectProperty<ExpandableSection, ObservableCollection<Control>> ControlsProperty =
        AvaloniaProperty.RegisterDirect<ExpandableSection, ObservableCollection<Control>>(
        nameof(Controls),
        o => o.Controls,
        (o, v) => o.Controls = v);

    private ObservableCollection<Control> _controls = [];

    public ObservableCollection<Control> Controls
    {
        get { return _controls; }
        set { SetAndRaise(ControlsProperty, ref _controls, value); }
    }

    protected override object Build() =>
        new Expander()
            .Padding(5)
            .MinHeight(20)
            .Styles([
                new Style(x => x.OfType<ToggleButton>())
                    .Setter(PaddingProperty, new Thickness(8))
                ])
            .DataContext(this)
            .IsExpanded(@IsExpanded)
            .Header(@Text)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Content(new ItemsControl()
                        .ItemsSource(@Controls)
            );
}
