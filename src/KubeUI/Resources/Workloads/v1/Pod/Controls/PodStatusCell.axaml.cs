using k8s;
using k8s.Models;
using Kubernetes.Controller.Client;
using KubeUI.Client;

namespace KubeUI.Resources.Workloads.v1.Pod.Controls;

public sealed partial class PodStatusCell : UserControl, IInitializeCluster
{
    private ICluster? _cluster;

    private V1Pod? _viewModel;

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; }

    [GeneratedDirectProperty]
    public partial IBrush Color { get; set; }

    public PodStatusCell()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetPrettyString();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _cluster?.OnChange -= _cluster_OnChange;
    }

    private void SetPrettyString()
    {
        if (DataContext is V1Pod pod)
        {
            _viewModel = pod;

            if (pod.Metadata?.DeletionTimestamp.HasValue == true)
            {
                PrettyString = "Terminating";
            }
            else
            {
                var ready = pod.Status?.Conditions?.FirstOrDefault(c => c.Type == "Ready");
                PrettyString = ready?.Status == "True"
                    ? "Running"
                    : ready?.Reason ?? "Unknown";
            }

            Color = PrettyString switch
            {
                "PodCompleted" or "Running" => new SolidColorBrush(Colors.LimeGreen),
                _ => new SolidColorBrush(Colors.Orange),
            };
        }
        else
        {
            PrettyString = string.Empty;
        }
    }

    private void _cluster_OnChange(WatchEventType arg1, GroupApiVersionKind arg2, IKubernetesObject<V1ObjectMeta> arg3)
    {
        if (_viewModel?.Name() == arg3.Name() && _viewModel?.Namespace() == arg3.Namespace())
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataContext = arg3;
                SetPrettyString();
            }, DispatcherPriority.Normal);
        }
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
        _cluster.OnChange += _cluster_OnChange;
    }
}
