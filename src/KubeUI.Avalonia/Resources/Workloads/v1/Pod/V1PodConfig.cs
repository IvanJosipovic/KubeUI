using Avalonia.Collections;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed partial class V1PodConfig : ResourceConfigBase<V1Pod>
{
    public V1PodConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

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
                    Key = "containers",
                    Name = Assets.Resources.V1PodConfig_Containers!,
                    CustomControl = typeof(PodContainerCell),
                    Field = x => x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0),
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                NamespaceColumn(),
                new ResourceListColumn<V1Pod, int>()
                {
                    Key = "restarts",
                    Name = Assets.Resources.V1PodConfig_Restarts!,
                    Field = x => x.Status.ContainerStatuses?.Sum(x => x.RestartCount) ?? 0,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Key = "controlled-by",
                    Name = Assets.Resources.V1PodConfig_Controlled_By!,
                    Field = x => x.Metadata.OwnerReferences?.FirstOrDefault()?.Name ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Key = "node",
                    Name = Assets.Resources.V1PodConfig_Node!,
                    Field = x => x.Spec.NodeName ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Key = "qos",
                    Name = Assets.Resources.V1PodConfig_QoS!,
                    Field = x => x.Status.QosClass ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
                new ResourceListColumn<V1Pod, string>()
                {
                    Key = "status",
                    Name = Assets.Resources.V1PodConfig_Status!,
                    Field = x => x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Ready")?.Status == "True" ? "Running" : x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "Unknown",
                    CustomControl = typeof(PodStatusCell),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];

        if (Cluster.IsMetricsAvailable)
        {
            cols.Insert(3, new ResourceListColumn<V1Pod, decimal>()
            {
                Key = "cpu",
                Name = Assets.Resources.V1PodConfig_CPU!,
                CustomControl = typeof(PodMetricCPUCell),
                Field = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]) ?? 0,
                Width = "80"
            });
            cols.Insert(4, new ResourceListColumn<V1Pod, decimal>()
            {
                Key = "memory",
                Name = Assets.Resources.V1PodConfig_Memory!,
                CustomControl = typeof(PodMetricMemoryCell),
                Field = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]) ?? 0,
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
        var ephemeralContainers = selectedItem?.Spec?.EphemeralContainers ?? [];

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
                    new()
                    {
                        Header = "Ephemeral",
                        Items = new AvaloniaList<MenuItemViewModel>(ephemeralContainers.Select(c => new MenuItemViewModel()
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
                Header = "Attach",
                FluentIcon = Icon.Link,
                Items = selectedItem == null ? null : new AvaloniaList<MenuItemViewModel>([
                    new()
                    {
                        Header = "Init",
                        Items = [.. initContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = AttachConsoleCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()],
                    },
                    new()
                    {
                        Header = "Normal",
                        Items = [.. containers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = AttachConsoleCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()],
                    },
                    new()
                    {
                        Header = "Ephemeral",
                        Items = [.. ephemeralContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = AttachConsoleCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()],
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
                    new()
                    {
                        Header = "Ephemeral",
                        Items = new AvaloniaList<MenuItemViewModel>(ephemeralContainers.Select(c => new MenuItemViewModel()
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
                Header = Assets.Resources.V1PodConfig_DebugContainer,
                FluentIcon = Icon.Code,
                Items = selectedItem == null ? null : new AvaloniaList<MenuItemViewModel>([
                    new()
                    {
                        Header = Assets.Resources.V1PodConfig_DebugContainer_Pod,
                        Command = DebugContainerCommand,
                        CommandParameter = new ArrayList { selectedItem },
                    },
                    new()
                    {
                        Header = "Init",
                        Items = [.. initContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = DebugContainerCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()],
                    },
                    new()
                    {
                        Header = "Normal",
                        Items = [.. containers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = DebugContainerCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()],
                    },
                    new()
                    {
                        Header = "Ephemeral",
                        Items = [.. ephemeralContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Command = DebugContainerCommand,
                            CommandParameter = new ArrayList { selectedItem, c },
                        }).ToList()],
                    },
                ]),
            },
            new()
            {
                Header = "Port Forwarding",
                FluentIcon = Icon.CloudFlow,
                Items = selectedItem == null ? null : new AvaloniaList<MenuItemViewModel>([
                    new()
                    {
                        Header = "Init",
                        Items = [.. initContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Items = [.. c.Ports?.Select(p => new MenuItemViewModel()
                            {
                                Header = $"{p.Name} - {p.ContainerPort}",
                                Command = PortForwardCommand,
                                CommandParameter = new ArrayList { selectedItem, p },
                            }).ToList() ?? []],
                        }).ToList()],
                    },
                    new()
                    {
                        Header = "Normal",
                        Items = [.. containers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Items = [.. c.Ports?.Select(p => new MenuItemViewModel()
                            {
                                Header = $"{p.Name} - {p.ContainerPort}",
                                Command = PortForwardCommand,
                                CommandParameter = new ArrayList { selectedItem, p },
                            }).ToList() ?? []],
                        }).ToList()],
                    },
                    new()
                    {
                        Header = "Ephemeral",
                        Items = [.. ephemeralContainers.Select(c => new MenuItemViewModel()
                        {
                            Header = c.Name,
                            Items = [.. c.Ports?.Select(p => new MenuItemViewModel()
                            {
                                Header = $"{p.Name} - {p.ContainerPort}",
                                Command = PortForwardCommand,
                                CommandParameter = new ArrayList { selectedItem, p },
                            }).ToList() ?? []],
                        }).ToList()],
                    },
                ]),
            }
        ];
    }

    public override IList<(Verb verb, string? subResource)> CustomPermissions() => [
        (Verb.Get, "log"),
        (Verb.Create, "portforward"),
        (Verb.Create, "exec"),
        (Verb.Create, "attach"),
        (Verb.Update, "ephemeralcontainers"),
    ];

    [RelayCommand(CanExecute = nameof(CanViewLogs))]
    private async Task ViewLogs(IList parameters)
    {
        if (parameters.Count != 2 || parameters[0] is not V1Pod pod)
        {
            return;
        }

        string? containerName = null;

        if (parameters[1] is V1Container container)
        {
            containerName = container.Name;
        }
        else if (parameters[1] is k8s.Models.V1EphemeralContainer ephemeral)
        {
            containerName = ephemeral.Name;
        }

        if (containerName == null)
        {
            return;
        }

        var vm = ServiceProvider.GetRequiredService<PodLogsViewModel>();
        vm.Cluster = Cluster;
        vm.Object = pod;
        vm.ContainerName = containerName;
        vm.Id = $"{nameof(ViewLogs)}-{Cluster.Name}-{pod.Namespace()} - {pod.Name()}-{containerName}";

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

    private bool CanViewLogs(IList? parameters)
    {
        if (parameters?.Count != 2)
        {
            return false;
        }

        if (parameters?[0] is V1Pod pod && (parameters?[1] is V1Container || parameters?[1] is k8s.Models.V1EphemeralContainer))
        {
            return Cluster.CanI<V1Pod>(Verb.Get, pod.Namespace(), "log");
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanViewConsole))]
    private void ViewConsole(IList parameters)
    {
        if (parameters?.Count != 2 || parameters[0] is not V1Pod pod)
        {
            return;
        }

        string? containerName = null;

        if (parameters[1] is V1Container container)
        {
            containerName = container.Name;
        }
        else if (parameters[1] is k8s.Models.V1EphemeralContainer ephemeral)
        {
            containerName = ephemeral.Name;
        }

        if (containerName == null)
        {
            return;
        }

        var vm = ServiceProvider.GetRequiredService<PodConsoleViewModel>();
        vm.Cluster = Cluster;
        vm.Object = pod;
        vm.ContainerName = containerName;
        vm.Id = $"{nameof(ViewConsole)}-{Cluster.Name}-{pod.Namespace()}-{pod.Name()}-{containerName}";

        _factory.AddToBottom(vm);
    }

    private bool CanViewConsole(IList? parameters)
    {
        return CanOpenConsole(parameters, "exec");
    }

    [RelayCommand(CanExecute = nameof(CanAttachConsole))]
    private void AttachConsole(IList parameters)
    {
        if (parameters?.Count != 2 || parameters[0] is not V1Pod pod)
        {
            return;
        }

        string? containerName = null;

        if (parameters[1] is V1Container container)
        {
            containerName = container.Name;
        }
        else if (parameters[1] is k8s.Models.V1EphemeralContainer ephemeral)
        {
            containerName = ephemeral.Name;
        }

        if (containerName == null)
        {
            return;
        }

        var vm = ServiceProvider.GetRequiredService<PodConsoleViewModel>();
        vm.Cluster = Cluster;
        vm.Object = pod;
        vm.ContainerName = containerName;
        vm.UseAttach = true;
        vm.Id = $"{nameof(AttachConsole)}-{Cluster.Name}-{pod.Namespace()}-{pod.Name()}-{containerName}";

        _factory.AddToBottom(vm);
    }

    private bool CanAttachConsole(IList? parameters)
    {
        return CanOpenConsole(parameters, "attach");
    }

    private bool CanOpenConsole(IList? parameters, string subResource)
    {
        if (parameters?.Count != 2)
        {
            return false;
        }

        if (parameters?[0] is V1Pod pod && (parameters?[1] is V1Container || parameters?[1] is k8s.Models.V1EphemeralContainer))
        {
            return Cluster.CanI<V1Pod>(Verb.Create, pod.Namespace(), subResource);
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanDebugContainer))]
    private async Task DebugContainer(IList parameters)
    {
        if (parameters.Count < 1 || parameters[0] is not V1Pod pod)
        {
            return;
        }

        string? targetContainerName = parameters.Count > 1 ? GetContainerName(parameters[1]) : null;
        string debugContainerImage = ServiceProvider.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(Cluster)
            .DebugContainerImage;

        if (string.IsNullOrWhiteSpace(debugContainerImage))
        {
            debugContainerImage = ClusterSettings.DefaultDebugContainerImage;
        }

        try
        {
            await Cluster.AddPodEphemeralDebugContainer(pod, targetContainerName, debugContainerImage);
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, Assets.Resources.V1PodConfig_DebugContainer_Error, sendNotification: true);
        }
    }

    private bool CanDebugContainer(IList? parameters)
    {
        if (parameters?.Count is not (1 or 2))
        {
            return false;
        }

        if (parameters[0] is not V1Pod pod)
        {
            return false;
        }

        if (parameters.Count == 2 && !IsContainerTarget(parameters[1]))
        {
            return false;
        }

        return Cluster.CanI<V1Pod>(Verb.Update, pod.Namespace(), "ephemeralcontainers");
    }

    [RelayCommand(CanExecute = nameof(CanPortForward))]
    private async Task PortForward(IList parameters)
    {
        if (parameters[0] is V1Pod pod && parameters[1] is V1ContainerPort containerPort)
        {
            var pf = Cluster.AddPodPortForward(pod.Namespace(), pod.Name(), containerPort.ContainerPort);

            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListView_PortForward_Title,
                Content = string.Format(Assets.Resources.ResourceListView_PortForward_Content, containerPort.ContainerPort, pf.LocalPort),
                PrimaryButtonText = Assets.Resources.ResourceListView_PortForward_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListView_PortForward_Secondary,
                DefaultButton = FAContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == FAContentDialogResult.Primary)
            {
                await App.TopLevel!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
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

    private static bool IsContainerTarget(object? value)
    {
        return value is V1Container || value is k8s.Models.V1EphemeralContainer || value is string;
    }

    private static string? GetContainerName(object? value)
    {
        return value switch
        {
            V1Container container => container.Name,
            k8s.Models.V1EphemeralContainer ephemeralContainer => ephemeralContainer.Name,
            string containerName => containerName,
            _ => null,
        };
    }

    public override Control[] Properties(V1Pod resource) => [new PropertiesView()];
}

