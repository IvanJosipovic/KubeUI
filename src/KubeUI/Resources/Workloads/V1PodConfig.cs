using Avalonia.Controls.Notifications;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Controls;
using KubeUI.Views;
using Scrutor;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Pod>>(ServiceLifetime.Transient)]
public sealed partial class V1PodConfig : ResourceConfigBase<V1Pod>, IInitializeCluster
{
    private readonly ILogger<V1DaemonSetConfig> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;
    private readonly IFactory _factory;

    private ICluster _cluster;

    public override string Category => "Workloads";

    public override int Order => 0;

    public V1PodConfig(ILogger<V1DaemonSetConfig> logger, IDialogService dialogService, INotificationManager notificationManager, IFactory factory)
    {
        _logger = logger;
        _dialogService = dialogService;
        _notificationManager = notificationManager;
        _factory = factory;
    }

    public override IList<IResourceListColumn> Columns()
    {
        List<IResourceListColumn> cols =
            [
                NameColumn(SortDirection.Ascending),
                new ResourceListColumn<V1Pod, int>()
                {
                    Name = "Containers",
                    CustomControl = typeof(PodContainerCell),
                    Field = x => x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0),
                    Display = x => (x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0)).ToString(),
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                NamespaceColumn(),
                new ResourceListColumn<V1Pod, int>()
                {
                    Name = "Restarts",
                    Field = x => x.Status.ContainerStatuses.Sum(x => x.RestartCount),
                    Display = x => x.Status.ContainerStatuses?.Sum(x => x.RestartCount).ToString() ?? "0",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "Controlled By",
                    Field = x => x.Metadata.OwnerReferences?.FirstOrDefault()?.Name ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "Node",
                    Field = x => x.Spec.NodeName ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "QoS",
                    Field = x => x.Status.QosClass ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase ?? "",
                    CustomControl = typeof(PodStatusCell),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];

        if (_cluster.IsMetricsAvailable)
        {
            cols.Insert(3, new ResourceListColumn<V1Pod, decimal>()
            {
                Name = "CPU",
                CustomControl = typeof(PodMetricCPUCell),
                Field = x => _cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]) ?? 0,
                Display = x => _cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]).ToString() ?? "",
                Width = "80"
            });
            cols.Insert(4, new ResourceListColumn<V1Pod, decimal>()
            {
                Name = "Memory",
                CustomControl = typeof(PodMetricMemoryCell),
                Field = x => _cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]) ?? 0,
                Display = x => _cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]).ToString() ?? "",
                Width = "80"
            });
        }

        return cols;
    }

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "View Console",
                IconResource = "desktop_regular",
                MenuItems =
                [
                    new()
                    {
                        Header = "Init",
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec.InitContainers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = new Binding(nameof(V1Container.Name)),
                            CommandPath = nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." + nameof(ViewConsoleCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                    new()
                    {
                        Header = "Normal",
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec.Containers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = new Binding(nameof(V1Container.Name)),
                            CommandPath =nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." +  nameof(ViewConsoleCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                ]
            },
            new()
            {
                Header = "View Logs",
                IconResource = "text_description_regular",
                MenuItems = [
                    new()
                    {
                        Header = "Init",
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec.InitContainers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                            CommandPath = nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." + nameof(ViewLogsCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                    new()
                    {
                        Header = "Normal",
                        ItemSourcePath =  Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec.Containers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                            CommandPath =  nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." + nameof(ViewLogsCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                ],
            },
            new()
            {
                Header = "Port Forwarding",
                ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                IconResource = "ic_fluent_cloud_flow_filled",
                ItemTemplate = new()
                {
                    HeaderBinding = new Binding(nameof(V1Container.Name)),
                    ItemSourcePath = nameof(V1Container.Ports),
                    ItemTemplate = new()
                    {
                        HeaderBinding = new MultiBinding()
                        {
                            Bindings =
                            [
                                new Binding(nameof(V1ContainerPort.Name)),
                                new Binding(nameof(V1ContainerPort.ContainerPort))
                            ],
                            StringFormat = "{0} - {1}"
                        },
                        CommandPath = nameof(ResourceListViewModel<V1Pod>.ResourceConfig) + "." + nameof(PortForwardCommand),
                        CommandParameterPath = ".",
                        CommandParameterAddSelectedItem = true,
                    }
                }
            }
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override Control[] Properties(V1Pod resource)
    {
        return [
            new PropertyItem()
                .Key("Controlled By")
                .Value(resource.Metadata.OwnerReferences.FirstOrDefault(x => x.Controller == true)?.Name ?? "N/A"),
            new PropertyItem()
                .Key("Status")
                .Value(resource.Status.Phase),
            new PropertyItem()
                .Key("Node")
                .Value(resource.Spec.NodeName),
            new PropertyItem()
                .Key("Pod IP")
                .Value(resource.Status.PodIP),
            new ExpandableSection()
                    .Text("Containers")
                    .IsExpanded(true)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(resource.Spec.Containers)
                            .ItemTemplate(new FuncDataTemplate<V1Container>((x,_) =>
                                new PropertyItem()
                                    .Key("Name")
                                    .Value(x.Name)
                            ))
                    ])
        ];
    }

    [RelayCommand(CanExecute = nameof(CanViewLogs))]
    private async Task ViewLogs(IList parameters)
    {
        if (parameters[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters[1] is V1Container container)
        {
            var vm = Application.Current.GetRequiredService<PodLogsViewModel>();
            vm.Cluster = _cluster;
            vm.Object = pod.Value;
            vm.ContainerName = container.Name;
            vm.Id = $"{nameof(ViewLogs)}-{_cluster.Name}-{pod.Key.Namespace} - {pod.Key.Name}-{container.Name}";

            if (_factory.AddToBottom(vm))
            {
                try
                {
                    await vm.Connect();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error viewing logs");
                    return;
                }
            }
        }
    }

    private bool CanViewLogs(IList? parameters)
    {
        if (parameters?[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters?[1] is V1Container container)
        {
            return _cluster.CanI<V1Pod>(Verb.Get, pod.Key.Namespace, "log");
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanViewConsole))]
    private void ViewConsole(IList parameters)
    {
        if (parameters?[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters?[1] is V1Container container)
        {
            var vm = Application.Current.GetRequiredService<PodConsoleViewModel>();
            vm.Cluster = _cluster;
            vm.Object = pod.Value;
            vm.ContainerName = container.Name;
            vm.Id = $"{nameof(ViewConsole)}-{_cluster.Name}-{pod.Key.Namespace}-{pod.Key.Name}-{container.Name}";

            _factory.AddToBottom(vm);
        }
    }

    private bool CanViewConsole(IList? parameters)
    {
        if (parameters?[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters?[1] is V1Container container)
        {
            return _cluster.CanI<V1Pod>(Verb.Create, pod.Key.Namespace, "exec");
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanPortForward))]
    private async Task PortForward(IList parameters)
    {
        if (parameters[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters[1] is V1ContainerPort containerPort)
        {
            var pf = _cluster.AddPodPortForward(pod.Key.Namespace, pod.Key.Name, containerPort.ContainerPort);

            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListViewModel_PortForward_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_PortForward_Content, containerPort.ContainerPort, pf.LocalPort),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_PortForward_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_PortForward_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
                await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
            }
        }
    }

    private bool CanPortForward(IList? parameters)
    {
        if (parameters?[0] is KeyValuePair<NamespacedName, V1Pod> pod && parameters?[1] is V1ContainerPort containerPort)
        {
            return containerPort.ContainerPort > 0 &&
                   containerPort.Protocol == "TCP" &&
                   _cluster.CanI<V1Pod>(Verb.Create, pod.Key.Namespace, "portforward");
        }

        return false;
    }
}
