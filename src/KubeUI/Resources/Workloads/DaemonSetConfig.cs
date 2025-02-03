using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1DaemonSet>>(ServiceLifetime.Transient)]
public sealed partial class DaemonSetConfig : ResourceConfigBase<V1DaemonSet>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override string Category => "Workloads";

    public override int Order => 2;

    public DaemonSetConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1DaemonSet, int>()
            {
                Name = "Pods",
                Display = x => x.Status.NumberReady.ToString(),
                Field = x => x.Status.NumberReady,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1DaemonSet, string>()
            {
                Name = "Node Selector",
                Display = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
                Field = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
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
                CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartDaemonSetCommand),
                CommandParameterPath = "SelectedItem.Value"
            },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override object? Properties(V1DaemonSet resource)
    {
        return null;
    }
}
