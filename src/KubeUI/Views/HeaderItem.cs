using Avalonia.Styling;
using k8s;

namespace KubeUI.Views;

public partial class HeaderItem : ViewBase
{
    public static readonly DirectProperty<HeaderItem, string> TextProperty =
        AvaloniaProperty.RegisterDirect<HeaderItem, string>(
        nameof(Text),
        o => o.Text,
        (o, v) => o.Text = v);

    private string _text = string.Empty;

    public string Text
    {
        get { return _text; }
        set { SetAndRaise(TextProperty, ref _text, value); }
    }

    public static readonly DirectProperty<HeaderItem, ObservableCollection<Control>> ControlsProperty =
        AvaloniaProperty.RegisterDirect<HeaderItem, ObservableCollection<Control>>(
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
        new StackPanel()
            .DataContext(this)
            .Children([
                new PropertyItem()
                    .Key(@Text)
                    .Value("")
                    .Styles([
                        new Style(x => x.OfType<SelectableTextBlock>())
                            .Setter(Border.BackgroundProperty, "SystemAltHighColor".GetDynamicResource())
                            .Setter(Border.PaddingProperty, new Thickness(8))
                        ]),
                new ItemsControl()
                    .ItemsSource(@Controls)
                ]);
}
