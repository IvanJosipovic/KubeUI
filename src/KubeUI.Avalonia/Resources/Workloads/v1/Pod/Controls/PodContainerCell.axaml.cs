using System.Globalization;
using Avalonia.Media;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
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
        if (DataContext is V1Pod pod)
        {
            _viewModel = pod;

            var coll = new ObservableCollection<ViewModel>();
            if (pod?.Status?.ContainerStatuses != null)
            {
                coll.AddRange(pod.Status.ContainerStatuses.Select(x =>
                {
                    var vm = new ViewModel()
                    {
                        Name = x.Name,
                        Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), null, CultureInfo.InvariantCulture),
                        Type = "Normal",
                        Status = GetStatusText(x),
                        Restarts = x.RestartCount
                    };

                    var spec = pod.Spec?.Containers?.FirstOrDefault(c => c.Name == x.Name);
                    if (spec != null)
                    {
                        vm.Image = spec.Image ?? string.Empty;
                    }

                    return vm;
                }));
            }
            if (pod?.Status?.InitContainerStatuses != null)
            {
                coll.AddRange(pod.Status.InitContainerStatuses.Select(x =>
                {
                    var vm = new ViewModel()
                    {
                        Name = x.Name,
                        // Mark init containers so the converter can render an init-specific palette.
                        Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), "init", CultureInfo.InvariantCulture),
                        Type = "Init",
                        Status = GetStatusText(x),
                        Restarts = x.RestartCount
                    };

                    var spec = pod.Spec?.InitContainers?.FirstOrDefault(c => c.Name == x.Name);
                    if (spec != null)
                    {
                        vm.Image = spec.Image ?? string.Empty;
                    }

                    return vm;
                }));
            }
            if (pod?.Status?.EphemeralContainerStatuses != null)
            {
                coll.AddRange(pod.Status.EphemeralContainerStatuses.Select(x =>
                {
                    var vm = new ViewModel()
                    {
                        Name = x.Name,
                        // Pass a parameter to the converter so it can render ephemeral containers
                        // with a distinct palette while preserving status semantics.
                        Brush = (IBrush)ContainerStatusToBrushConverter.Instance().Convert(x, typeof(IBrush), "ephemeral", CultureInfo.InvariantCulture),
                        Type = "Ephemeral",
                        Status = GetStatusText(x),
                        Restarts = x.RestartCount
                    };

                    var spec = pod.Spec?.EphemeralContainers?.FirstOrDefault(c => c.Name == x.Name);
                    if (spec != null)
                    {
                        vm.Image = spec.Image ?? string.Empty;
                    }

                    return vm;
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

    private static string GetStatusText(V1ContainerStatus status)
    {
        try
        {
            if (status.State?.Running != null)
            {
                return "Running";
            }

            if (status.State?.Waiting != null)
            {
                var waiting = status.State.Waiting;
                var reason = !string.IsNullOrWhiteSpace(waiting.Reason) ? waiting.Reason : null;
                var msg = !string.IsNullOrWhiteSpace(waiting.Message) ? waiting.Message : null;
                return reason ?? msg ?? "Waiting";
            }

            var terminated = status.State?.Terminated;
            if (terminated != null)
            {
                if (terminated.Reason == "Completed")
                    return "Completed";

                return !string.IsNullOrWhiteSpace(terminated.Reason) ? terminated.Reason : "Terminated";
            }

            if (status.Ready && status.Started == true)
                return "Running";

            if (status.Started == true)
                return "Starting";

            return "Stopped";
        }
        catch
        {
            return "Unknown";
        }
    }

    public partial class ViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; }
        [ObservableProperty]
        public partial IBrush Brush { get; set; }

        [ObservableProperty]
        public partial string Type { get; set; }

        [ObservableProperty]
        public partial string Status { get; set; }

        [ObservableProperty]
        public partial int Restarts { get; set; }

        [ObservableProperty]
        public partial string Image { get; set; }
    };
}



