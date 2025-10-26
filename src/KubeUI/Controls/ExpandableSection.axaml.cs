using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Utils;

namespace KubeUI.Controls;

public partial class ExpandableSection : UserControl
{
    public static readonly DirectProperty<ExpandableSection, bool> IsExpandedProperty =
        AvaloniaProperty.RegisterDirect<ExpandableSection, bool>(
            nameof(IsExpanded),
            o => o.IsExpanded,
            (o, v) => o.IsExpanded = v);

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndRaise(IsExpandedProperty, ref _isExpanded, value);
    }

    public static readonly DirectProperty<ExpandableSection, string> TextProperty =
        AvaloniaProperty.RegisterDirect<ExpandableSection, string>(
            nameof(Text),
            o => o.Text,
            (o, v) => o.Text = v);

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    public static readonly DirectProperty<ExpandableSection, ObservableCollection<Control>> ControlsProperty =
        AvaloniaProperty.RegisterDirect<ExpandableSection, ObservableCollection<Control>>(
            nameof(Controls),
            o => o.Controls,
            (o, v) => o.Controls = v);

    private ObservableCollection<Control> _controls = [];

    public ObservableCollection<Control> Controls
    {
        get => _controls;
        set => SetAndRaise(ControlsProperty, ref _controls, value);
    }

    public ExpandableSection()
    {
        InitializeComponent();
        DataContext = this;
    }
}

public static class ExpandableSectionExtensions
{
    public static ExpandableSection Text(this ExpandableSection prop, string text)
    {
        prop.Text = text;
        return prop;
    }

    public static ExpandableSection IsExpanded(this ExpandableSection prop, bool value)
    {
        prop.IsExpanded = value;
        return prop;
    }

    public static ExpandableSection Controls(this ExpandableSection prop, Control[] controls)
    {
        prop.Controls.AddRange(controls);
        return prop;
    }
}
