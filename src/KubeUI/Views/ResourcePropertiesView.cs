using k8s.Models;
using k8s;
using Avalonia.Controls.Templates;
using System.Text;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace KubeUI.Views;

public sealed class ResourcePropertiesView<T> : MyViewBase<ResourcePropertiesViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected override StyleGroup? BuildStyles() => [
        new Style(x => x.OfType<PropertyItem>())
            .Setter(MarginProperty, new Thickness(4,0,0,10))
        ];

    protected override object Build(ResourcePropertiesViewModel<T>? vm)
    {
        if (vm == null)
            return new StackPanel();

        var sp = new StackPanel()
            .Children([
                new PropertyItem()
                    .Key("Name")
                    .Value(@vm.Object.Metadata.Name),
                new PropertyItem()
                    .Key("Namespace")
                    .Value(@vm.Object.Metadata.NamespaceProperty),
                new PropertyItem()
                    .Key("Created")
                    .Value(vm.Object.Metadata.CreationTimestamp.ToString()),
                ]);

        if (typeof(T) == typeof(V1Secret))
        {
            var obj = vm.Object as V1Secret;

            sp.Children.AddRange([
                new Separator(),
                new ItemsControl()
                    .ItemsSource(@obj.Data)
                    .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((x,_) =>
                        new PropertyItem()
                            .Key(x.Key)
                            .Value(Encoding.UTF8.GetString(x.Value))
                    ))
            ]);
        }
        return new ScrollViewer()
                .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                .Content(sp);
    }
}

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

    protected override StyleGroup? BuildStyles() => [];

    protected override object Build() =>
        new Grid()
            .DataContext(this)
            .Cols("*,2*")
            .VerticalAlignment(VerticalAlignment.Top)
            .Children([
                new SelectableTextBlock()
                    .Row(0).Col(0)
                    .Text(@Key)
                    .TextWrapping(TextWrapping.Wrap),
                new ScrollViewer()
                    .Row(0).Col(1)
                    .MaxHeight(200)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .Content(new SelectableTextBlock()
                        .Text(@Value)
                        .TextWrapping(TextWrapping.Wrap)
                    ),
                ]);
}
