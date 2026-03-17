using Humanizer;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Controls;

public partial class PodMetricMemoryCell : UserControl, IInitializeCluster
{
    private ICluster? _cluster;

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public PodMetricMemoryCell()
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

        var metric = _cluster.PodMetrics.FirstOrDefault(m =>
            m.Name() == pod.Name() && m.Namespace() == pod.Namespace());

        if (metric == null)
        {
            return;
        }

        try
        {
            var usageBytes = metric.Containers.Sum(c => c.Usage["memory"].ToInt64());
            PrettyString = usageBytes.Bytes().Humanize();
        }
        catch
        {
            PrettyString = string.Empty;
        }
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

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }
}
