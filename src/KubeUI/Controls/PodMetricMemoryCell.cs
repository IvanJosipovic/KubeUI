using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Views;

namespace KubeUI.Controls;

public sealed class PodMetricMemoryCell : MyViewBase<V1Pod>, IInitializeCluster
{
    public ICluster Cluster { get; set; }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public PodMetricMemoryCell()
    {
        s_timer.Tick += Timer_Tick;
        s_timer.Interval = TimeSpan.FromSeconds(1);
        s_timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Update();
    }

    public static readonly DirectProperty<PodMetricMemoryCell, string> PrettyStringProperty =
    AvaloniaProperty.RegisterDirect<PodMetricMemoryCell, string>(
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
                var usage = metric.Containers.Sum(x => x.Usage["memory"].ToInt64());
                PrettyString = usage.Bytes().Humanize();
            }
        }
    }

    protected override StyleGroup? BuildStyles() => [];

    protected override object Build(V1Pod? vm) =>
        new TextBlock()
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(PodMetricMemoryCell.PrettyStringProperty);

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        s_timer.Tick -= Timer_Tick;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Update();
    }
}
