using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1PriorityClass>>(ServiceLifetime.Transient)]
public sealed partial class PriorityClassConfig : ResourceConfigBase<V1PriorityClass>
{
    public override string Category => "Configuration";
    public override bool ShowNamespaces => false;
    public override int Order => 6;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1PriorityClass, int>()
            {
                Name = "Value",
                Display = x => x.Value.ToString(),
                Field = x => x.Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<V1PriorityClass, bool?>()
            {
                Name = "Global Default",
                Display = x => (x.GlobalDefault ?? false).ToString(),
                Field = x => x.GlobalDefault ?? false,
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

    public override Control[] Properties(V1PriorityClass resource)
    {
        return null;
    }
}
