using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace KubeUI.Controls;

public partial class HeaderItem : UserControl
{
    public static readonly DirectProperty<HeaderItem, string> TextProperty =
        AvaloniaProperty.RegisterDirect<HeaderItem, string>(
            nameof(Text),
            o => o.Text,
            (o, v) => o.Text = v);

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    public static readonly DirectProperty<HeaderItem, ObservableCollection<Control>> ControlsProperty =
        AvaloniaProperty.RegisterDirect<HeaderItem, ObservableCollection<Control>>(
            nameof(Controls),
            o => o.Controls,
            (o, v) => o.Controls = v);

    private ObservableCollection<Control> _controls = [];
    public ObservableCollection<Control> Controls
    {
        get => _controls;
        set => SetAndRaise(ControlsProperty, ref _controls, value);
    }

    public HeaderItem()
    {
        InitializeComponent();
        DataContext = this;
    }
}
