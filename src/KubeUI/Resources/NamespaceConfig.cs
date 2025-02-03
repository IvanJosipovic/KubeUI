using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Namespace>>(ServiceLifetime.Transient)]
public sealed partial class NamespaceConfig : ResourceConfigBase<V1Namespace>
{
    public override bool ShowNamespaces => false;
    public override int Order => 6;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
                NameColumn(SortDirection.Ascending),
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Labels",
                    Field = x => x.Metadata.Labels.Select(x => x.Key + "=" + x.Value).Aggregate((x,y) => x + ", " + y),
                    Width = "2*"
                },
                new ResourceListViewDefinitionColumn<V1Namespace, string>()
                {
                    Name = "Status",
                    Field = x => x.Status.Phase,
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

    public override Control[]? Properties(V1Namespace resource)
    {
        return null;
    }
}
