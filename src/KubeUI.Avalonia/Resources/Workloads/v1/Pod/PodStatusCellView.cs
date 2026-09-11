using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Styles;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class PodStatusCellView : RefreshingCellTextBlock, IInitializeCluster
{
    public ClusterWorkspace? Cluster { get; private set; }

    private V1Pod? _viewModel;

    private GroupApiVersionKind _groupApiVersionKind = GroupApiVersionKind.From<V1Pod>();
    private bool _isSubscribed;

    public PodStatusCellView()
        : base(null)
    {
    }

    protected override string ResolveText(object? dataContext)
    {
        if (dataContext is not V1Pod pod)
        {
            Foreground = ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
            return string.Empty;
        }

        _viewModel = pod;
        var status = pod.Metadata?.DeletionTimestamp.HasValue == true
            ? AppResources.PodStatusCell_Terminating!
            : pod.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "Ready") is { } ready
                ? ready.Status == "True"
                    ? AppResources.PodStatusCell_Running!
                    : ready.Reason == "PodCompleted"
                        ? AppResources.PodStatusCell_PodCompleted!
                        : ready.Reason ?? AppResources.PodStatusCell_Unknown!
                : AppResources.PodStatusCell_Unknown!;

        Foreground = status == AppResources.PodStatusCell_PodCompleted || status == AppResources.PodStatusCell_Running
            ? ApplicationBrushResources.GetBrush("PodStatusReadyBrush")
            : ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
        return status;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        UnsubscribeFromCluster();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToCluster();
    }

    private void _cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (_groupApiVersionKind == groupApiVersionKind && _viewModel?.Name() == resource.Name() && _viewModel?.Namespace() == resource.Namespace())
        {
            Dispatcher.UIThread.Invoke(() => DataContext = resource, DispatcherPriority.Normal);
        }
    }

    /// <summary>
    /// Initializes this cell with its cluster. Runtime change subscriptions start when the cell is attached.
    /// Reinitialization replaces the previous cluster subscription when the cell is attached.
    /// </summary>
    public void Initialize(ClusterWorkspace cluster)
    {
        UnsubscribeFromCluster();
        Cluster = cluster;
        if (VisualRoot != null)
        {
            SubscribeToCluster();
        }
    }

    private void SubscribeToCluster()
    {
        if (!_isSubscribed && Cluster != null)
        {
            Cluster.Runtime.OnChange += _cluster_OnChange;
            _isSubscribed = true;
        }
    }

    private void UnsubscribeFromCluster()
    {
        if (_isSubscribed && Cluster != null)
        {
            Cluster.Runtime.OnChange -= _cluster_OnChange;
            _isSubscribed = false;
        }
    }
}
