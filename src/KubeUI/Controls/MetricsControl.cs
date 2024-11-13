using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Views;

namespace KubeUI.Controls;

public sealed class MetricsControl : ViewBase
{
    public static readonly DirectProperty<MetricsControl, ICluster> ClusterProperty =
        AvaloniaProperty.RegisterDirect<MetricsControl, ICluster>(
        nameof(Cluster),
        o => o.Cluster,
        (o, v) => o.Cluster = v);

    private ICluster _cluster;

    public ICluster Cluster
    {
        get { return _cluster; }
        set { SetAndRaise(ClusterProperty, ref _cluster, value); }
    }

    public static readonly DirectProperty<MetricsControl, IKubernetesObject<V1ObjectMeta>> ResourceProperty =
    AvaloniaProperty.RegisterDirect<MetricsControl, IKubernetesObject<V1ObjectMeta>>(
    nameof(Resource),
    o => o.Resource,
    (o, v) => o.Resource = v);

    private IKubernetesObject<V1ObjectMeta> _resource;

    public IKubernetesObject<V1ObjectMeta> Resource
    {
        get { return _resource; }
        set { SetAndRaise(ResourceProperty, ref _resource, value); }
    }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public MetricsControl()
    {
        s_timer.Tick += Timer_Tick;
        s_timer.Interval = TimeSpan.FromSeconds(1);
        s_timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {

    }

    protected override StyleGroup? BuildStyles() => [];

    protected override object Build() =>
        new TextBlock()
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text("test");

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        s_timer.Tick -= Timer_Tick;
    }
}
