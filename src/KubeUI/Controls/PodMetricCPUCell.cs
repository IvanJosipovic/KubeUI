using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Views;

namespace KubeUI.Controls;

public sealed class PodMetricCPUCell : MyViewBase<V1Pod>, IInitializeCluster
{
    public ICluster Cluster { get; set; }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public PodMetricCPUCell()
    {
        s_timer.Tick += Timer_Tick;
        s_timer.Interval = TimeSpan.FromSeconds(1);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        Update();

        if (!s_timer.IsEnabled)
        {
            s_timer.Start();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Update();
    }

    public static readonly DirectProperty<PodMetricCPUCell, string> PrettyStringProperty =
    AvaloniaProperty.RegisterDirect<PodMetricCPUCell, string>(
    nameof(PrettyString),
    o => o.PrettyString,
    (o, v) => o.PrettyString = v);

    public string PrettyString
    {
        get { return _prettyString; }
        set { SetAndRaise(PrettyStringProperty, ref _prettyString, value); }
    }

    private string _prettyString = string.Empty;

    private void Update()
    {
        if (Cluster != null)
        {
            var metric = Cluster.PodMetrics.FirstOrDefault(x => x.Name() == ViewModel.Name() && x.Namespace() == ViewModel.Namespace());
            if (metric != null)
            {
                var usage = metric.Containers.Sum(x => x.Usage["cpu"].ToDecimal());
                PrettyString = $"{usage:F3}c";
            }
        }
    }

    protected override StyleGroup? BuildStyles() => [];

    protected override object Build(V1Pod? vm) =>
        new TextBlock()
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(PodMetricCPUCell.PrettyStringProperty);

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        s_timer.Tick -= Timer_Tick;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
    }
}
