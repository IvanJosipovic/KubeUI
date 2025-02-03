using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1DaemonSet>>(ServiceLifetime.Transient)]
public sealed partial class V1DaemonSetConfig : ResourceConfigBase<V1DaemonSet>
{
    public override string Category => "Workloads";

    public override int Order => 2;

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

    public override Control[]? Properties(V1DaemonSet resource)
    {
        return null;
    }
}
