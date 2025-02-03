using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1StatefulSet>>(ServiceLifetime.Transient)]
public sealed partial class StatefulSetConfig : ResourceConfigBase<V1StatefulSet>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override string Category => "Workloads";

    public override int Order => 3;

    public StatefulSetConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1StatefulSet, int>()
            {
                Name = "Replicas",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.Replicas,
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
                CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartStatefulSetCommand),
                CommandParameterPath = "SelectedItem.Value"
            },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override object? Properties(V1StatefulSet resource)
    {
        return null;
    }
}
