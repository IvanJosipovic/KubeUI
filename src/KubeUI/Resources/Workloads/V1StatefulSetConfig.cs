using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1StatefulSet>>(ServiceLifetime.Transient)]
public sealed partial class V1StatefulSetConfig : ResourceConfigBase<V1StatefulSet>
{
    public override string Category => "Workloads";

    public override int Order => 3;

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

    public override Control[]? Properties(V1StatefulSet resource)
    {
        return null;
    }
}
