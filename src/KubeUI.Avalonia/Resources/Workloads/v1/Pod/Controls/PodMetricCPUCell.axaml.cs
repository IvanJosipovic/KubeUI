using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;

public partial class PodMetricCPUCell : UserControl, IInitializeCluster
{
    private ClusterWorkspaceViewModel? _cluster;

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public PodMetricCPUCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Update();
    }

    private void Timer_Tick(object? sender, EventArgs e) => Update();

    private void Update()
    {
        if (_cluster == null || DataContext is not V1Pod pod)
        {
            return;
        }

        var metric = _cluster.PodMetrics.FirstOrDefault(x =>
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

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        _cluster = cluster;
    }
}


