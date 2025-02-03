using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1ConfigMap>>(ServiceLifetime.Transient)]
public sealed partial class ConfigMapConfig : ResourceConfigBase<V1ConfigMap>
{
    public override string Category => "Configuration";
    public override int Order => 0;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1ConfigMap, string>()
            {
                Name = "Keys",
                Display = x => x.Data != null && x.Data.Keys.Count > 0 ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1ConfigMap resource)
    {
        return null;
    }
}
