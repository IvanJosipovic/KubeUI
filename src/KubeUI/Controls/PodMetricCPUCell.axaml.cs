using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Controls;

public partial class PodMetricCPUCell : UserControl, IInitializeCluster
{
    public ICluster? Cluster { get; private set; }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    static PodMetricCPUCell()
    {
        s_timer.Interval = TimeSpan.FromSeconds(1);
        if (!s_timer.IsEnabled)
            s_timer.Start();
    }

    public PodMetricCPUCell()
    {
        InitializeComponent();
        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Update();
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

    private string _prettyString = string.Empty;

    public string PrettyString
    {
        get => _prettyString;
        private set => SetAndRaise(PrettyStringProperty, ref _prettyString, value);
    }

    private void Update()
    {
        if (Cluster == null)
        {
            PrettyString = string.Empty;
            return;
        }

        if (DataContext is not V1Pod pod)
        {
            PrettyString = string.Empty;
            return;
        }

        // Locate matching pod metric
        var metric = Cluster.PodMetrics.FirstOrDefault(x =>
            x.Name() == pod.Name() && x.Namespace() == pod.Namespace());

        if (metric == null)
        {
            PrettyString = string.Empty;
            return;
        }

        // Sum container CPU usage; assumes "cpu" exists and ToDecimal() extension is available
        var usage = metric.Containers.Sum(c => c.Usage["cpu"].ToDecimal());
        PrettyString = $"{usage:F3}c";
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
