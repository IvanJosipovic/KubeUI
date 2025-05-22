using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Controls;
using KubeUI.Views;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads.Pod;

public sealed partial class V1PodConfig : ResourceConfigBase<V1Pod>
{
    public override string Category => "Workloads";

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
                    FieldExpression = x => x.Spec.Containers.Count + (x.Spec.InitContainers == null ? 0 : x.Spec.InitContainers.Count),
                    Width = "auto"
                },
                NamespaceColumn(),
                new ResourceListColumn<V1Pod, int>()
                {
                    Name = "Restarts",
                    FieldExpression = x => x.Status.ContainerStatuses.Sum(x => x.RestartCount),
                    Width = "auto"
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "Controlled By",
                    FieldExpression = x => x.Metadata.OwnerReferences != null && x.Metadata.OwnerReferences.Any() ? x.Metadata.OwnerReferences[0].Name : "",
                    Width = "auto"
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "Node",
                    FieldExpression = x => x.Spec.NodeName,
                    Width = "auto"
                },
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "QoS",
                    FieldExpression = x => x.Status.QosClass,
                    Width = "auto"
                },
                AgeColumn(),
                new ResourceListColumn<V1Pod, string>()
                {
                    Name = "Status",
                    FieldExpression = x => x.Status.Phase,
                    CustomControl = typeof(PodStatusCell),
                    Width = "auto"
                },
            ];

        //if (Cluster.IsMetricsAvailable)
        //{
        //    cols.Insert(3, new ResourceListColumn<V1Pod, decimal>()
        //    {
        //        Name = "CPU",
        //        CustomControl = typeof(PodMetricCPUCell),
        //        Field = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]) ?? 0,
        //        Display = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["cpu"]).ToString() ?? "",
        //        Width = "80"
        //    });
        //    cols.Insert(4, new ResourceListColumn<V1Pod, decimal>()
        //    {
        //        Name = "Memory",
        //        CustomControl = typeof(PodMetricMemoryCell),
        //        Field = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]) ?? 0,
        //        Display = x => Cluster.PodMetrics.FirstOrDefault(y => y.Name() == x.Name() && y.Namespace() == x.Namespace())?.Containers.Sum(z => z.Usage["memory"]).ToString() ?? "",
        //        Width = "80"
        //    });
        //}

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
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.InitContainers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                            CommandPath = nameof(ViewConsoleCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                    new()
                    {
                        Header = "Normal",
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.Containers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                            CommandPath =  nameof(ViewConsoleCommand),
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
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.InitContainers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                            CommandPath = nameof(ViewLogsCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                    new()
                    {
                        Header = "Normal",
                        ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.Containers),
                        ItemTemplate = new()
                        {
                            HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                            CommandPath =  nameof(ViewLogsCommand),
                            CommandParameterPath = ".",
                            CommandParameterAddSelectedItem = true,
                        }
                    },
                ],
            },
            new()
            {
                Header = "Port Forwarding",
                ItemSourcePath = Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.Containers),
                IconResource = "ic_fluent_cloud_flow_filled",
                ItemTemplate = new()
                {
                    HeaderBinding = Utilities.FuncBinding<V1Container>(x => x.Name),
                    ItemSourcePath = Utilities.PathBuilder<V1Container>(x => x.Ports),
                    ItemTemplate = new()
                    {
                        HeaderBinding = new MultiBinding()
                        {
                            Bindings =
                            [
                                Utilities.FuncBinding<V1ContainerPort>(x => x.Name),
                                Utilities.FuncBinding<V1ContainerPort>(x => x.ContainerPort),
                            ],
                            StringFormat = "{0} - {1}"
                        },
                        CommandPath = nameof(PortForwardCommand),
                        CommandParameterPath = ".",
                        CommandParameterAddSelectedItem = true,
                    }
                }
            }
        ];
    }

    public override Control[] Properties(V1Pod resource)
    {
        return [
            new PropertyItem()
                .Key("Controlled By")
                .Value(@resource.Metadata.OwnerReferences.FirstOrDefault(x => x.Controller == true)?.Name ?? "N/A"),
            new PropertyItem()
                .Key("Status")
                .Value(@resource.Status.Phase),
            new PropertyItem()
                .Key("Node")
                .Value(@resource.Spec.NodeName),
            new PropertyItem()
                .Key("Pod IP")
                .Value(@resource.Status.PodIP),
            new ExpandableSection()
                    .Text("Init Containers")
                    .IsExpanded(true)
                    .IsVisible(() => resource.Spec.InitContainers?.Count > 0)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(resource.Spec.InitContainers)
                            .ItemTemplate(new FuncDataTemplate<V1Container>((x,_) =>
                            new StackPanel()
                                    .Children([
                                        new PropertyItem().Key("Name").Value(@x.Name),
                                        //new PropertyItem().Key("Status").Value(@x.Name),
                                        new PropertyItem().Key("Image").Value(@x.Image),
                                        new PropertyItem().Key("Image Pull Policy").Value(@x.ImagePullPolicy),
                                    ])
                            ))
                        ]),
            new ExpandableSection()
                    .Text("Containers")
                    .IsExpanded(true)
                    .Controls([
                        new ItemsControl()
                            .ItemsSource(resource.Spec.Containers)
                            .ItemTemplate(new FuncDataTemplate<V1Container>((x,_) =>
                            new StackPanel()
                                    .Children([
                                        new PropertyItem().Key("Name").Value(@x.Name),
                                        //new PropertyItem().Key("Status").Value(this.Name),
                                        new PropertyItem().Key("Image").Value(@x.Image),
                                        new PropertyItem().Key("Image Pull Policy").Value(@x.ImagePullPolicy),
                                    ])
                            ))
                    ])
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
        if (parameters?[0] is V1Pod pod && parameters?[1] is V1ContainerPort containerPort)
        {
            return containerPort.ContainerPort > 0 &&
                   containerPort.Protocol == "TCP" &&
                   Cluster.CanI<V1Pod>(Verb.Create, pod.Namespace(), "portforward");
        }

        return false;
    }
}
