using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public partial class PodMetricCPUCellView : ViewBase<V1Pod>, IInitializeCluster
{
    public ClusterWorkspace? Cluster { get; private set; }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public PodMetricCPUCellView()
    {
        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }
    }

    protected override object Build(V1Pod vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new TextBlock()
            .Name("CellTextBlock")
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Text(this, x => x.PrettyString)
            .ToolTip_Tip(this, x => x.PrettyString);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Update();
    }

    private void Timer_Tick(object? sender, EventArgs e) => Update();

    private void Update()
    {
        if (Cluster == null || DataContext is not V1Pod pod)
        {
            return;
        }

        var metric = Cluster.Runtime.PodMetrics.FirstOrDefault(x =>
            x.Name() == pod.Name() && x.Namespace() == pod.Namespace());

        if (metric == null)
        {
            return;
        }

        var usage = metric.Containers.Sum(c => c.Usage["cpu"].ToDecimal());
        PrettyString = $"{usage:F3}c";
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}
