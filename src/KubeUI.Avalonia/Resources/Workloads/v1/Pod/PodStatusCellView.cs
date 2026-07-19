using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed partial class PodStatusCellView : ViewBase<V1Pod>, IInitializeCluster
{
    public ClusterWorkspace? Cluster { get; private set; }

    private V1Pod? _viewModel;

    private GroupApiVersionKind _groupApiVersionKind = GroupApiVersionKind.From<V1Pod>();

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    [GeneratedDirectProperty]
    public partial IBrush Color { get; set; } = new SolidColorBrush(Colors.Orange);

    protected override object Build(V1Pod vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new TextBlock()
            .Name("CellTextBlock")
            .Margin(12, 0, 12, 0)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .VerticalAlignment(VerticalAlignment.Center)
            .Foreground(this, x => x.Color)
            .Text(this, x => x.PrettyString)
            .ToolTip_Tip(this, x => x.PrettyString);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetPrettyString();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Cluster?.Runtime.OnChange -= _cluster_OnChange;
    }

    private void SetPrettyString()
    {
        if (DataContext is V1Pod pod)
        {
            _viewModel = pod;

            if (pod.Metadata?.DeletionTimestamp.HasValue == true)
            {
                PrettyString = AppResources.PodStatusCell_Terminating!;
            }
            else
            {
                var ready = pod.Status?.Conditions?.FirstOrDefault(c => c.Type == "Ready");

                PrettyString = ready?.Status == "True"
                    ? AppResources.PodStatusCell_Running!
                    : ready?.Reason == "PodCompleted"
                        ? AppResources.PodStatusCell_PodCompleted!
                        : ready?.Reason ?? AppResources.PodStatusCell_Unknown!;
            }

            Color = PrettyString switch
            {
                var status when status == AppResources.PodStatusCell_PodCompleted || status == AppResources.PodStatusCell_Running => new SolidColorBrush(Colors.LimeGreen),
                _ => new SolidColorBrush(Colors.Orange),
            };
        }
        else
        {
            PrettyString = string.Empty;
            Color = new SolidColorBrush(Colors.Orange);
        }
    }

    private void _cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (_groupApiVersionKind == groupApiVersionKind && _viewModel?.Name() == resource.Name() && _viewModel?.Namespace() == resource.Namespace())
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataContext = resource;
                SetPrettyString();
            }, DispatcherPriority.Normal);
        }
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Cluster.Runtime.OnChange += _cluster_OnChange;
    }
}
