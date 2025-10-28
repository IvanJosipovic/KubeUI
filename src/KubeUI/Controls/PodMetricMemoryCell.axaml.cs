using Humanizer;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Controls;

public partial class PodMetricMemoryCell : UserControl, IInitializeCluster
{
    public ICluster? Cluster { get; private set; }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public PodMetricMemoryCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }

        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Update();
    }

    private void Timer_Tick(object? sender, EventArgs e) => Update();

    public static readonly DirectProperty<PodMetricMemoryCell, string> PrettyStringProperty =
        AvaloniaProperty.RegisterDirect<PodMetricMemoryCell, string>(
            nameof(PrettyString),
            o => o.PrettyString,
            (o, v) => o.PrettyString = v);

    private string _prettyString = string.Empty;
    public string PrettyString
    {
        get => _prettyString;
        private set => SetAndRaise(PrettyStringProperty, ref _prettyString, value);
    }

    private void Update()
    {
        if (Cluster == null || DataContext is not V1Pod pod)
        {
            PrettyString = string.Empty;
            return;
        }

        var metric = Cluster.PodMetrics.FirstOrDefault(m =>
            m.Name() == pod.Name() && m.Namespace() == pod.Namespace());

        if (metric == null)
        {
            PrettyString = string.Empty;
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Update();
    }
}
