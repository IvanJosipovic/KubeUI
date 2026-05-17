using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap;

public sealed partial class V1ConfigMapConfig : ResourceConfigBase<V1ConfigMap>
{
    public V1ConfigMapConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Configuration!;
    public override int Order => 0;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ConfigMap, string>()
            {
                Key = "keys",
                Name = Assets.Resources.V1ConfigMapConfig_Keys!,
                Field = x => x.Data is { Count: > 0 } data ? string.Join(", ", data.Keys) : "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ConfigMap resource) => [new PropertiesView()];
}
