using System.Globalization;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public partial class PodContainerCellView : ViewBase<V1Pod>, IInitializeCluster
{
    public ClusterWorkspace? Cluster { get; private set; }

    private V1Pod? _viewModel;

    private GroupApiVersionKind _groupApiVersionKind = GroupApiVersionKind.From<V1Pod>();

    [GeneratedDirectProperty]
    public partial ObservableCollection<ContainerStatusViewModel> ContainerStatuses { get; set; } = [];

    public PodContainerCellView()
    {
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
                        new()
                        {
                            Name = AppResources.PodContainerCell_Starting,
                            Started = true,
                            Ready = false,
                        },
                        new()
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
                        new()
                        {
                            Name = AppResources.PodContainerCell_InitStarting,
                            Started = true,
                            Ready = false,
                        },
                        new()
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

    protected override object Build(V1Pod vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new ItemsControl()
            .Margin(10, 0, 0, 0)
            .ItemsSource(this, x => x.ContainerStatuses)
            .ItemsPanel(new ItemsPanelTemplate()
            {
                Content = (IServiceProvider? _) =>
                    new TemplateResult<Control>(
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                        , new NameScope())
            })
            .ItemTemplate(new FuncDataTemplate<ContainerStatusViewModel>((vm, _) =>
                new Ellipse()
                    .Fill(vm.Brush)
                    .Width(10)
                    .Height(10)
                    .Stroke(Brushes.Gray)
                    .StrokeThickness(1)
                    .Margin(0, 0, 4, 0)
                    .ToolTip_Tip(
                        new StackPanel()
                            .Children(
                                new TextBlock()
                                    .Text(string.Format(AppResources.PodContainerCell_NameFormat, vm.Name)),
                                new TextBlock()
                                    .Text(string.Format(AppResources.PodContainerCell_TypeFormat, vm.Type)),
                                new TextBlock()
                                    .Text(string.Format(AppResources.PodContainerCell_StatusFormat, vm.Status)),
                                new TextBlock()
                                    .Text(string.Format(AppResources.PodContainerCell_RestartsFormat, vm.Restarts)),
                                new TextBlock()
                                    .Text(string.Format(AppResources.PodContainerCell_ImageFormat, vm.Image))))));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        PopulateData();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Cluster?.Runtime.OnChange -= _cluster_OnChange;
    }

    private void PopulateData()
    {
        ContainerStatuses.Clear();

        if (DataContext is V1Pod pod)
        {
            _viewModel = pod;

            if (pod.Status?.ContainerStatuses != null)
            {
                foreach (var status in pod.Status.ContainerStatuses)
                {
                    var image = pod.Spec?.Containers?.FirstOrDefault(c => c.Name == status.Name)?.Image;
                    ContainerStatuses.Add(CreateViewModel(status, "Normal", null, image));
                }
            }

            if (pod.Status?.InitContainerStatuses != null)
            {
                foreach (var status in pod.Status.InitContainerStatuses)
                {
                    var image = pod.Spec?.InitContainers?.FirstOrDefault(c => c.Name == status.Name)?.Image;
                    ContainerStatuses.Add(CreateViewModel(status, "Init", "init", image));
                }
            }

            if (pod.Status?.EphemeralContainerStatuses != null)
            {
                foreach (var status in pod.Status.EphemeralContainerStatuses)
                {
                    var image = pod.Spec?.EphemeralContainers?.FirstOrDefault(c => c.Name == status.Name)?.Image;
                    ContainerStatuses.Add(CreateViewModel(status, "Ephemeral", "ephemeral", image));
                }
            }
        }
    }

    private static ContainerStatusViewModel CreateViewModel(V1ContainerStatus status, string type, string? converterParameter, string? image)
    {
        ContainerStatusViewModel vm = new()
        {
            Name = status.Name,
            Brush = ContainerStatusToBrushConverter.Instance().Convert(status, typeof(IBrush), converterParameter, CultureInfo.InvariantCulture) as IBrush ?? Brushes.Gray,
            Type = type,
            Status = GetStatusText(status),
            Restarts = status.RestartCount,
        };

        if (!string.IsNullOrEmpty(image))
        {
            vm.Image = image;
        }

        return vm;
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
                {
                    return "Completed";
                }

                return !string.IsNullOrWhiteSpace(terminated.Reason) ? terminated.Reason : "Terminated";
            }

            if (status.Ready && status.Started == true)
            {
                return "Running";
            }

            if (status.Started == true)
            {
                return "Starting";
            }

            return "Stopped";
        }
        catch
        {
            return "Unknown";
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

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Cluster.Runtime.OnChange += _cluster_OnChange;
    }

    public sealed partial class ContainerStatusViewModel : ObservableObject
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
    }
}
