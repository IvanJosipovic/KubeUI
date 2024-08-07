using k8s.Models;
using k8s;
using Avalonia.Controls.Templates;
using System.Text;

namespace KubeUI.Views;

public sealed class ResourcePropertiesView<T> : MyViewBase<ResourcePropertiesViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected override StyleGroup? BuildStyles() => [];

    protected override object Build(ResourcePropertiesViewModel<T>? vm)
    {
        if (vm == null) return new StackPanel();

        var sp = new StackPanel()
            .Children([
                new PropertyItem()
                    .Key("Name:")
                    .Value(@vm.Object.Metadata.Name),
                new PropertyItem()
                    .Key("Namespace:")
                    .Value(@vm.Object.Metadata.NamespaceProperty),
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
        return new Grid()
            .Children(sp);
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
            .Cols("*,3*")
            .VerticalAlignment(VerticalAlignment.Top)
            .Children([
                new SelectableTextBlock()
                    .Row(0).Col(0)
                    .Text(@Key),
                new SelectableTextBlock()
                    .Row(0).Col(1)
                    .Text(@Value),
                ]);
}
