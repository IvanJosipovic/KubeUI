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
                        .Value(@vm.Object.Metadata.Name),
                    new PropertyItem()
                        .Key("Namespace")
                        .Value(@vm.Object.Metadata.NamespaceProperty),
                    new PropertyItem()
                        .Key("Created")
                        .Set(PropertyItem.ValueProperty, @vm.Object.Metadata.CreationTimestamp, converter: new ((x) => x.HasValue ? x.Value.ToLocalTime().ToString() : "" )),
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
