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
            //.Setter(MarginProperty, new Thickness(0,0,0,15))
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
                        .Set(PropertyItem.ValueProperty, @vm.Object.Metadata.CreationTimestamp, converter: new ((x) => x.HasValue ? x.Value.ToLocalTime().ToString() : "" )),
            ]);

        if (typeof(T) == typeof(V1Secret))
        {
            var obj = vm.Object as V1Secret;

            sp.Children.Add(
                new ExpandableSection()
                    .Text("Data")
                    .IsExpanded(true)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(@obj.Data)
                            .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((x,_) =>
                                new PropertyItem()
                                    .Key(@x.Key)
                                    .Value(Encoding.UTF8.GetString(x.Value))
                            ))
                    ])
            );

            sp.Children.Add(
                new ExpandableSection()
                    .Text("Certificates")
                    .IsExpanded(true)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(@obj.Data)
                            .ItemTemplate(new FuncDataTemplate<KeyValuePair<string, byte[]>>((x,_) =>
                                new CertificateItemView()
                                    .Header(@x.Key)
                                    .Bytes(@x.Value)
                            ))
                    ])
            );
        }
        return new ScrollViewer()
                .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                .Content(sp);
    }
}

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
