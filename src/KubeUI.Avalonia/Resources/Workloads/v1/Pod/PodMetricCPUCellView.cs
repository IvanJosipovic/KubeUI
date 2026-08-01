using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public partial class PodMetricCPUCellView : ViewBase<V1Pod>, IInitializeCluster
{
    public ClusterWorkspace? Cluster { get; private set; }

    private readonly IUiRefreshClock _refreshClock;
    private IDisposable? _refreshSubscription;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public PodMetricCPUCellView(IUiRefreshClock refreshClock)
    {
        _refreshClock = refreshClock;
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
        _refreshSubscription = _refreshClock.Subscribe(Update);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}
