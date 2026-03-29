using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using System.Globalization;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Converters;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;

public partial class PodContainerCell : UserControl, IInitializeCluster
{
    private ClusterWorkspaceViewModel? _cluster;

    private V1Pod? _viewModel;

    private GroupApiVersionKind _groupApiVersionKind = GroupApiVersionKind.From<V1Pod>();

    [GeneratedDirectProperty]
    public partial ObservableCollection<ViewModel> ContainerStatuses { get; set; } = [];

    public PodContainerCell()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Pod()
            {
                Status = new()
                {
                    ContainerStatuses = new ObservableCollection<V1ContainerStatus>()
                    {
                        new()
                        {
                            Name = AppResources.PodContainerCell_Stopped,
                            Started = false,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = AppResources.PodContainerCell_Starting,
                            Started = true,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = AppResources.PodContainerCell_Ready,
                            Started = true,
                            Ready = true,
                        },
                    },
                    InitContainerStatuses = new ObservableCollection<V1ContainerStatus>()
                    {
                        new()
                        {
                            Name = AppResources.PodContainerCell_InitStopped,
                            Started = false,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = AppResources.PodContainerCell_InitStarting,
                            Started = true,
                            Ready = false,
                        },
                        new ()
                        {
                            Name = AppResources.PodContainerCell_InitReady,
                            Started = true,
                            Ready = true,
                        },
                    }
                }
            };
        }
#endif
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        PopulateData();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _cluster?.OnChange -= _cluster_OnChange;
    }

    private void PopulateData()
    {
        if(DataContext is V1Pod pod)
        {
            _viewModel = pod;

            var coll = new ObservableCollection<ViewModel>();
            if (pod?.Status?.ContainerStatuses != null)
            {
                coll.AddRange(pod.Status.ContainerStatuses.Select(x => new ViewModel()
                {
                    Name = x.Name,
                    Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), null, CultureInfo.InvariantCulture)
                }));
            }
            if (pod?.Status?.InitContainerStatuses != null)
            {
                coll.AddRange(pod.Status.InitContainerStatuses.Select(x => new ViewModel()
                {
                    Name = x.Name,
                    Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), null, CultureInfo.InvariantCulture)
                }));
            }
            ContainerStatuses = coll;
        }
    }

    private void _cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (_groupApiVersionKind == groupApiVersionKind && _viewModel?.Name() == resource.Name() && _viewModel?.Namespace() == resource.Namespace())
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataContext = resource;
                PopulateData();
            }, DispatcherPriority.Normal);
        }
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        _cluster = cluster;
        _cluster.OnChange += _cluster_OnChange;
    }

    public partial class ViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; }

        [ObservableProperty]
        public partial IBrush Brush { get; set; }
    };
}



