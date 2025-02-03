using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Controls;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Pod>>(ServiceLifetime.Transient)]
public sealed partial class PodConfig : ResourceConfigBase<V1Pod>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override string Category => "Workloads";

    public PodConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1Pod, int>()
                {
                    Name = "Containers",
                    CustomControl = typeof(PodContainerCell),
                    Field = x => x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0),
                    Display = x => (x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0)).ToString(),
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Pod, int>()
                {
                    Name = "Restarts",
                    Field = x => x.Status.ContainerStatuses.Sum(x => x.RestartCount),
                    Display = x => x.Status.ContainerStatuses?.Sum(x => x.RestartCount).ToString() ?? "0",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Controlled By",
                    Field = x => x.Metadata.OwnerReferences?.FirstOrDefault(x => x.Controller == true)?.Name ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Node",
                    Field = x => x.Spec.NodeName ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "QoS",
                    Field = x => x.Status.QosClass ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
                new ResourceListViewDefinitionColumn<V1Pod, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase ?? "",
                    CustomControl = typeof(PodStatusCell),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
            ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
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
                        }
                    },
                ],
            },
            //new()
            //{
            //    Header = "Port Forwarding",
            //    ItemSourcePath = "SelectedItem.Value.Spec.Containers",
            //    IconResource = "ic_fluent_cloud_flow_filled",
            //    ItemTemplate = new()
            //    {
            //        HeaderBinding = new Binding(nameof(V1Container.Name)),
            //        ItemSourcePath = nameof(V1Container.Ports),
            //        ItemTemplate = new()
            //        {
            //            HeaderBinding = new MultiBinding()
            //            {
            //                Bindings =
            //                [
            //                    new Binding(nameof(V1ContainerPort.Name)),
            //                    new Binding(nameof(V1ContainerPort.ContainerPort))
            //                ],
            //                StringFormat = "{0} - {1}"
            //            },
            //            CommandPath = nameof(ResourceListViewModel<V1Pod>.PortForwardCommand),
            //            CommandParameterPath = ".",
            //        }
            //    }
            //}
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override object? Properties(V1Pod resource)
    {
        return null;
    }

    [RelayCommand(CanExecute = nameof(CanViewLogs))]
    private async Task ViewLogs(IList parameters)
    {
        var pod = parameters[0] as V1Pod;
        var container = parameters[1] as V1Container;

        var vm = Application.Current.GetRequiredService<PodLogsViewModel>();
        vm.Cluster = _cluster;
        vm.Object = pod;
        vm.ContainerName = container.Name;
        vm.Id = $"{nameof(ViewLogs)}-{_cluster.Name}-{pod.Namespace()}-{pod.Name()}-{container.Name}";

        if (_factory.AddToBottom(vm))
        {
            try
            {
                await vm.Connect();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error viewing logs");
                return;
            }
        }
    }

    private bool CanViewLogs(IList parameters)
    {
        return parameters != null;
        //return container != null && Cluster.CanI<V1Pod>(Verb.Get, ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Namespace, "log");
    }

    [RelayCommand(CanExecute = nameof(CanViewConsole))]
    private void ViewConsole(IList parameters)
    {
        var pod = (V1Pod)parameters[0];
        var container = (V1Container)parameters[1];

        var vm = Application.Current.GetRequiredService<PodConsoleViewModel>();
        vm.Cluster = _cluster;
        vm.Object = pod;
        vm.ContainerName = container.Name;
        vm.Id = $"{nameof(ViewConsole)}-{_cluster.Name}-{pod.Namespace()}-{pod.Name()}-{container.Name}";

        _factory.AddToBottom(vm);
    }

    private bool CanViewConsole(IList parameters)
    {
        if (parameters == null)
        {
            return false;
        }

        var pod = parameters[0] as V1Pod;
        var container = parameters[1] as V1Container;

        return container != null && pod != null && _cluster.CanI<V1Pod>(Verb.Create, pod.Namespace(), "exec");
    }

    //[RelayCommand(CanExecute = nameof(CanPortForward))]
    //private async Task PortForward(V1ContainerPort containerPort)
    //{
    //    var pf = Cluster.AddPodPortForward(((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Namespace, ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Name, containerPort.ContainerPort);

    //    ContentDialogSettings settings = new()
    //    {
    //        Title = Resources.ResourceListViewModel_PortForward_Title,
    //        Content = string.Format(Resources.ResourceListViewModel_PortForward_Content, containerPort.ContainerPort, pf.LocalPort),
    //        PrimaryButtonText = Resources.ResourceListViewModel_PortForward_Primary,
    //        SecondaryButtonText = Resources.ResourceListViewModel_PortForward_Secondary,
    //        DefaultButton = ContentDialogButton.Secondary
    //    };

    //    var result = await _dialogService.ShowContentDialogAsync(this, settings);

    //    if (result == ContentDialogResult.Primary)
    //    {
    //        var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
    //        await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
    //    }
    //}

    //private bool CanPortForward(V1ContainerPort? containerPort)
    //{
    //    return containerPort?.ContainerPort > 0 &&
    //           containerPort.Protocol == "TCP" &&
    //           Cluster.CanI<V1Pod>(Verb.Create, ((KeyValuePair<NamespacedName, V1Pod>)SelectedItem).Key.Namespace, "portforward");
    //}

    //[RelayCommand(CanExecute = nameof(CanPortForwardService))]
    //private async Task PortForwardService(V1ServicePort containerPort)
    //{
    //    var pf = Cluster.AddServicePortForward(((KeyValuePair<NamespacedName, V1Service>)SelectedItem).Key.Namespace, ((KeyValuePair<NamespacedName, V1Service>)SelectedItem).Key.Name, containerPort.Port);

    //    ContentDialogSettings settings = new()
    //    {
    //        Title = Resources.ResourceListViewModel_PortForward_Title,
    //        Content = string.Format(Resources.ResourceListViewModel_PortForward_Content, containerPort.Port, pf.LocalPort),
    //        PrimaryButtonText = Resources.ResourceListViewModel_PortForward_Primary,
    //        SecondaryButtonText = Resources.ResourceListViewModel_PortForward_Secondary,
    //        DefaultButton = ContentDialogButton.Secondary
    //    };

    //    var result = await _dialogService.ShowContentDialogAsync(this, settings);

    //    if (result == ContentDialogResult.Primary)
    //    {
    //        var window = (Window)_dialogService.DialogManager.GetMainWindow()!.RefObj;
    //        await window!.Launcher.LaunchUriAsync(new Uri($"http://localhost:{pf.LocalPort}"));
    //    }
    //}

    //private bool CanPortForwardService(V1ServicePort? servicePort)
    //{
    //    var @namespace = ((KeyValuePair<NamespacedName, V1Service>)SelectedItem).Key.Namespace;

    //    return servicePort?.Port > 0 &&
    //           servicePort.Protocol == "TCP" &&
    //           Cluster.CanI<V1Pod>(Verb.Create, @namespace, "portforward") &&
    //           Cluster.CanI<V1Endpoints>(Verb.List, @namespace) &&
    //           Cluster.CanI<V1Endpoints>(Verb.Watch, @namespace);
    //}
}
