using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1RuntimeClass>>(ServiceLifetime.Transient)]
public sealed partial class RuntimeClassConfig : ResourceConfigBase<V1RuntimeClass>
{
    public override string Category => "Configuration";
    public override bool ShowNamespaces => false;
    public override int Order => 7;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1RuntimeClass, string>()
            {
                Name = "Handler",
                Display = x => x.Handler,
                Field = x => x.Handler,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1RuntimeClass resource)
    {
        return null;
    }
}
