using k8s.Models;
using Avalonia.Controls;
using KubeUI.Avalonia.Resources.Configuration.v1.RuntimeClass.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.RuntimeClass;

public sealed partial class V1RuntimeClassConfig : ResourceConfigBase<V1RuntimeClass>
{
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
    public override int Order => 7;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1RuntimeClass, string>()
            {
                Name = "Handler",
                Field = x => x.Handler,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1RuntimeClass resource) => [new PropertiesView()];
}

