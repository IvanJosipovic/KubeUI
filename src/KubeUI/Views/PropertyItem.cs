using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using k8s;

namespace KubeUI.Views;

public partial class PropertyItem : ViewBase
{
    public static readonly DirectProperty<PropertyItem, string> KeyProperty =
    AvaloniaProperty.RegisterDirect<PropertyItem, string>(
    nameof(Key),
    o => o.Key,
    (o, v) => o.Key = v);

    private string _key = string.Empty;

    public string Key
    {
        get { return _key; }
        set { SetAndRaise(KeyProperty, ref _key, value); }
    }

    public static readonly DirectProperty<PropertyItem, string> ValueProperty =
    AvaloniaProperty.RegisterDirect<PropertyItem, string>(
    nameof(Value),
    o => o.Value,
    (o, v) => o.Value = v);

    public string Value
    {
        get { return _value; }
        set { SetAndRaise(ValueProperty, ref _value, value); }
    }

    private string _value = string.Empty;

    protected override StyleGroup? BuildStyles() => [
        new Style(x => x.OfType<SelectableTextBlock>())
            .Setter(PaddingProperty, new Thickness(5))
        ];

    protected override object Build() =>
        new Grid()
            .DataContext(this)
            .Cols("*,2*")
            .VerticalAlignment(VerticalAlignment.Top)
            .Children([
                new SelectableTextBlock()
                    .Row(0).Col(0)
                    .Text(@Key)
                    .TextWrapping(TextWrapping.NoWrap)
                    .ToolTip(@Key),
                new ScrollViewer()
                    .Row(0).Col(1)
                    .MaxHeight(200)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .Content(new SelectableTextBlock()
                                .Text(@Value)
                                .TextWrapping(TextWrapping.Wrap)
                    ),
            ])
        ;
}
