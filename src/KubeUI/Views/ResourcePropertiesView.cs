using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Views;

public sealed class ResourcePropertiesView<T> : MyViewBase<ResourcePropertiesViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected override StyleGroup? BuildStyles() => [
        new Style(x => x.OfType<PropertyItem>())
            //.Setter(MarginProperty, new Thickness(0,0,0,15))
        ];

    protected override object Build(ResourcePropertiesViewModel<T>? vm)
    {
        if (vm == null || vm.Object == null)
            return new StackPanel();

        var sp = new StackPanel()
            .Children([
                    new PropertyItem()
                        .Key("Name")
                        .Value(vm.Object.Metadata.Name),
                    new PropertyItem()
                        .Key("Namespace")
                        .Value(vm.Object.Metadata.NamespaceProperty),
                    new PropertyItem()
                        .Key("Created")
                        .Value(vm.Object.Metadata.CreationTimestamp?.ToLocalTime().ToString() ?? ""),
            ]);

        var props = vm.ResourceConfig.Properties(vm.Object);

        if (props != null)
        {
            sp.Children.AddRange(props);
        }

        return new ScrollViewer()
                .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                .Content(sp);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel != null)
        {
            ViewModel.Cluster.OnChange += Cluster_OnChange;
        }
    }

    private async void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (ViewModel?.Object != null
                && ViewModel.Object.Kind == resource.Kind
                && ViewModel.Object.ApiVersion == resource.ApiVersion
                && ViewModel.Object.Metadata.Name == resource.Metadata.Name
                && ViewModel.Object.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty)
            {
                ViewModel.Object = (T)resource;
                Reload();
            }
        });
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (ViewModel != null)
        {
            ViewModel.Cluster.OnChange -= Cluster_OnChange;
        }
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
            .IsExpanded(IsExpandedProperty)
            .Header(TextProperty)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Content(new ItemsControl()
                        .ItemsSource(ControlsProperty)
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

    protected override object Build()
    {
        return new StackPanel()
            .DataContext(this)
            .Children([
                new PropertyItem()
                    .Set<PropertyItem, string>(PropertyItem.KeyProperty, TextProperty)
                    .Styles([
                        new Style(x => x.OfType<SelectableTextBlock>())
                            .Setter(Border.BackgroundProperty, "SystemAltHighColor".GetDynamicResource())
                            .Setter(Border.PaddingProperty, new Thickness(8))
                        ]),
                new ItemsControl()
                    .ItemsSource(ControlsProperty)
                ]);
    }
}

public partial class PropertyItem : ViewBase
{
    public string? Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public static readonly StyledProperty<string?> KeyProperty = AvaloniaProperty.Register<PropertyItem, string?>(nameof(Key));

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<PropertyItem, string?>(nameof(Value));

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
                    .Text(KeyProperty)
                    .TextWrapping(TextWrapping.NoWrap)
                    .ToolTip(KeyProperty),
                new ScrollViewer()
                    .Row(0).Col(1)
                    .MaxHeight(200)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .Content(new SelectableTextBlock()
                                .Text(ValueProperty)
                                .TextWrapping(TextWrapping.Wrap)
                    ),
            ])
        ;
}
