using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1ReplicaSet>>(ServiceLifetime.Transient)]
public sealed partial class ReplicaSetConfig : ResourceConfigBase<V1ReplicaSet>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override string Category => "Workloads";

    public override int Order => 4;

    public ReplicaSetConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1ReplicaSet, int>()
            {
                Name = "Desired",
                Display = x => (x.Spec.Replicas ?? 0).ToString(),
                Field = x => x.Spec.Replicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1ReplicaSet, int>()
            {
                Name = "Current",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.Replicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1ReplicaSet, int>()
            {
                Name = "Ready",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.ReadyReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "Restart",
                IconResource = "arrow_sync_regular",
                CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartReplicaSetCommand),
                CommandParameterPath = "SelectedItem.Value"
            },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override object? Properties(V1ReplicaSet resource)
    {
        return null;
    }
}
