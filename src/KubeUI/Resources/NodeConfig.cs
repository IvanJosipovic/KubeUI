using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Node>>(ServiceLifetime.Transient)]
public sealed partial class NodeConfig : ResourceConfigBase<V1Node>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;
    public new bool ShowNewResource => false;

    public new bool ShowNamespaces => false;

    public NodeConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Instance Type",
                    Field = x => x.Metadata.Labels.TryGetValue("node.kubernetes.io/instance-type", out var value) ? value : "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Node, decimal>()
                {
                    Name = "CPU",
                    Field = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal() : 0,
                    Display = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal().ToString("0.##") + "c" : "0c",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, decimal>()
                {
                    Name = "Memory",
                    Field = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDecimal() : 0,
                    Display = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0Gi",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, decimal>()
                {
                    Name = "Disk",
                    Field = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? value.ToDecimal() : 0,
                    Display = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Taints",
                    Field = x => x?.Spec?.Taints?.Select(x => $"{x.Key}={x.Effect}").Aggregate((x,y) => $"{x}, {y}") ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Version",
                    Field = x => x.Status.NodeInfo.KubeletVersion,
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Node, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Conditions.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToCells)
                },
                AgeColumn(),
            ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [
                new()
                {
                    Header = "Cordon",
                    IconResource = "stop_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.CordonNodeCommand),
                    CommandParameterPath = "SelectedItems",
                },
                new()
                {
                    Header = "UnCordon",
                    IconResource = "play_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.UnCordonNodeCommand),
                    CommandParameterPath = "SelectedItems",
                },
                new()
                {
                    Header = "Drain",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Pod>.DrainNodeCommand),
                    CommandParameterPath = "SelectedItems",
                },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override object? Properties(V1Node resource)
    {
        return null;
    }
}
