using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1Service>>(ServiceLifetime.Transient)]
public sealed partial class ServiceConfig : ResourceConfigBase<V1Service>
{
    public override string Category => "Network";
    public override int Order => 0;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1Service, string>()
            {
                Name = "Type",
                Display = x => x.Spec.Type,
                Field = x => x.Spec.Type,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<V1Service, string>()
            {
                Name = "Cluster IP",
                Display = x => x.Spec.ClusterIP,
                Field = x => x.Spec.ClusterIP,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<V1Service, int>()
            {
                Name = "Ports",
                Display = x => x.Spec.Ports.Select((a) => $"{a.Port}{(string.IsNullOrEmpty(a.Name) ? "" : ":" + a.Name)}/{a.Protocol}").Aggregate((a,b) => a + ", " + b),
                Field = x => x.Spec.Ports.FirstOrDefault().Port,
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
                Header = "Port Forwarding",
                ItemSourcePath = "SelectedItem.Value.Spec.Ports",
                IconResource = "ic_fluent_cloud_flow_filled",
                ItemTemplate = new()
                {
                    HeaderBinding = new MultiBinding()
                    {
                        Bindings =
                        [
                            new Binding(nameof(V1ServicePort.Name)),
                            new Binding(nameof(V1ServicePort.Port))
                        ],
                        StringFormat = "{0} - {1}"
                    },
                    //CommandPath = nameof(ResourceListViewModel<V1Pod>.PortForwardServiceCommand), //todo fix
                    CommandParameterPath = ".",
                }
            }
        ];
    }

    public override Control[]? Properties(V1Service resource)
    {
        return null;
    }
}
