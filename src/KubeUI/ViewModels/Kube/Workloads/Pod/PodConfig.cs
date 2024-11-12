using k8s.Models;
using KubeUI.Controls;

namespace KubeUI.ViewModels.Kube.Workloads.Pod;

internal sealed class PodConfig : ResourceViewModelBase<V1Pod>
{
    public override IList<IResourceListViewDefinitionColumn> Columns { get; } =
    [
        ResourceListViewModel<V1Pod>.NameColumn(SortDirection.Ascending),
        new ResourceListViewDefinitionColumn<V1Pod, int>()
        {
            Name = "Containers",
            CustomControl = typeof(PodContainerCell),
            Field = x => x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0),
            Display = x => (x.Spec.Containers.Count + ((x.Spec.InitContainers?.Count) ?? 0)).ToString(),
            Width = nameof(DataGridLengthUnitType.SizeToCells)
        },
        ResourceListViewModel<V1Pod>.NamespaceColumn(),
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
        ResourceListViewModel<V1Pod>.AgeColumn(),
        new ResourceListViewDefinitionColumn<V1Pod, string>()
        {
            Name = "Status",
            Field = x => x.Status.Phase ?? "",
            CustomControl = typeof(PodStatusCell),
            Width = nameof(DataGridLengthUnitType.SizeToHeader)
        },
    ];

    public override IList<ResourceListViewMenuItem> MenuItems { get; } =
    [
        new()
        {
            Header = "View Console",
            IconResource = "desktop_regular",
            MenuItems =
            [
                new()
                {
                    Header = "Init",
                    ItemSourcePath = "SelectedItem.Value.Spec.InitContainers",
                    ItemTemplate = new()
                    {
                        HeaderBinding = new Binding(nameof(V1Container.Name)),
                        CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewConsoleCommand),
                        CommandParameterPath = ".",
                    }
                },
                new()
                {
                    Header = "Normal",
                    ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                    ItemTemplate = new()
                    {
                        HeaderBinding = new Binding(nameof(V1Container.Name)),
                        CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewConsoleCommand),
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
                    ItemSourcePath = "SelectedItem.Value.Spec.InitContainers",
                    ItemTemplate = new()
                    {
                        HeaderBinding = new Binding(nameof(V1Container.Name)),
                        CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewLogsCommand),
                        CommandParameterPath = ".",
                    }
                },
                new()
                {
                    Header = "Normal",
                    ItemSourcePath = "SelectedItem.Value.Spec.Containers",
                    ItemTemplate = new()
                    {
                        HeaderBinding = new Binding(nameof(V1Container.Name)),
                        CommandPath = nameof(ResourceListViewModel<V1Pod>.ViewLogsCommand),
                        CommandParameterPath = ".",
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
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.PortForwardCommand),
                    CommandParameterPath = ".",
                }
            }
        }
    ];

    public override object? Properties(V1Pod resource)
    {
        return null;
    }
}
