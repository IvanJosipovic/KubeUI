using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;
using Avalonia.Collections;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed partial class V1PodConfig : ResourceConfigBase<V1Pod>
{
    public override bool IsNamespaced => true;

    public override string Category => CategoryString("ResourceConfig_Category_Workloads", "Workloads");

    public override int Order => 0;

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
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                NamespaceColumn(),
                new ResourceListColumn<V1Pod, int>()
                {
                    Name = "Restarts",
                    Field = x => x.Status.ContainerStatuses?.Sum(x => x.RestartCount) ?? 0,
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
                    Field = x => x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Ready")?.Status == "True" ? "Running" : x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "Unknown",
                    CustomControl = typeof(PodStatusCell),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];

        if (Cluster.IsMetricsAvailable)
        {
            cols.Insert(3, new ResourceListColumn<V1Pod, decimal>()
            {
                Name = "CPU",
                CustomControl = typeof(PodMetricCPUCell),
                Field = x => Cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer
                    ? Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]) ?? 0
                    : 0,
                Width = "80"
            });
            cols.Insert(4, new ResourceListColumn<V1Pod, decimal>()
            {
                Name = "Memory",
                CustomControl = typeof(PodMetricMemoryCell),
                Field = x => Cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer
                    ? Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]) ?? 0
                    : 0,
                Width = "80"
            });
        }

        return cols;
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Pod>? selectedItems)
    {
        var selectedList = selectedItems?.ToList();
        if (selectedList == null || selectedList.Count != 1)
        {
            return [];
        }

        var selectedItem = selectedList[0];
        var initContainers = selectedItem?.Spec?.InitContainers ?? [];
        var containers = selectedItem?.Spec?.Containers ?? [];

        return [
            new()
            {
                Header = "View Console",
                FluentIcon = Icon.Desktop,
                Items = selectedItem == null ? null : new AvaloniaList<MenuItemViewModel>([
                    new()
                    {
                        Header = "Init",
                        Items = new AvaloniaList<MenuItemViewModel>(initContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = ViewConsoleCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()),
                    },
                    new()
                    {
                        Header = "Normal",
                        Items = new AvaloniaList<MenuItemViewModel>(containers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = ViewConsoleCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()),
                    },
                ]),
            },
            new()
            {
                Header = "View Logs",
                FluentIcon = Icon.TextDescription,
                Items = selectedItem == null ? null : new AvaloniaList<MenuItemViewModel>([
                    new()
                    {
                        Header = "Init",
                        Items = new AvaloniaList<MenuItemViewModel>(initContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = ViewLogsCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()),
                    },
                    new()
                    {
                        Header = "Normal",
                        Items = new AvaloniaList<MenuItemViewModel>(containers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = ViewLogsCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()),
                    },
                ]),
            },
            new()
            {
                Header = "Port Forwarding",
                FluentIcon = Icon.CloudFlow,
                Items = selectedItem == null ? null : new AvaloniaList<MenuItemViewModel>(containers.Select(c => new MenuItemViewModel()
                {
                    Header = c.Name,
                    Items = new AvaloniaList<MenuItemViewModel>(c.Ports?.Select(p => new MenuItemViewModel()
                    {
                        Header = $"{p.Name} - {p.ContainerPort}",
                        Command = PortForwardCommand,
                        CommandParameter = new ArrayList { selectedItem, p },
                    }).ToList() ?? []),
                }).ToList()),
            }
        ];
    }

    public override IList<(Verb verb, string? subResource)> CustomPermissions() => [
        (Verb.Get, "log"),
        (Verb.Create, "portforward"),
        (Verb.Create, "exec"),
    ];

    [RelayCommand(CanExecute = nameof(CanViewLogs))]
    private async Task ViewLogs(IList parameters)
    {
        if (parameters[0] is V1Pod pod && parameters[1] is V1Container container)
        {
            var vm = Application.Current.GetRequiredService<PodLogsViewModel>();
            vm.Cluster = Cluster;
            vm.Object = pod;
            vm.ContainerName = container.Name;
            vm.Id = $"{nameof(ViewLogs)}-{Cluster.Name}-{pod.Namespace()} - {pod.Name()}-{container.Name}";

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
        if (parameters?.Count != 2)
        {
            return false;
        }

        if (parameters?[0] is V1Pod pod && parameters?[1] is V1Container container)
        {
            return Cluster.CanI<V1Pod>(Verb.Get, pod.Namespace(), "log");
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanViewConsole))]
    private void ViewConsole(IList parameters)
    {
        if (parameters?[0] is V1Pod pod && parameters?[1] is V1Container container)
        {
            var vm = Application.Current.GetRequiredService<PodConsoleViewModel>();
            vm.Cluster = Cluster;
            vm.Object = pod;
            vm.ContainerName = container.Name;
            vm.Id = $"{nameof(ViewConsole)}-{Cluster.Name}-{pod.Namespace()}-{pod.Name()}-{container.Name}";

            _factory.AddToBottom(vm);
        }
    }

    private bool CanViewConsole(IList? parameters)
    {
        if (parameters?.Count != 2)
        {
            return false;
        }

        if (parameters?[0] is V1Pod pod && parameters?[1] is V1Container container)
        {
            return Cluster.CanI<V1Pod>(Verb.Create, pod.Namespace(), "exec");
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanPortForward))]
    private async Task PortForward(IList parameters)
    {
        if (parameters[0] is V1Pod pod && parameters[1] is V1ContainerPort containerPort)
        {
            var pf = Cluster.AddPodPortForward(pod.Namespace(), pod.Name(), containerPort.ContainerPort);

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
        if (parameters?.Count != 2)
        {
            return false;
        }

        if (parameters?[0] is V1Pod pod && parameters?[1] is V1ContainerPort containerPort)
        {
            return containerPort.ContainerPort > 0 &&
                   containerPort.Protocol == "TCP" &&
                   Cluster.CanI<V1Pod>(Verb.Create, pod.Namespace(), "portforward");
        }

        return false;
    }

    public override Control[] Properties(V1Pod resource) => [new PropertiesView()];
}



