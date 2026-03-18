using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration;

public sealed partial class V1ConfigMapConfig : ResourceConfigBase<V1ConfigMap>
{
    public override bool IsNamespaced => true;
    public override string Category => "Configuration";
    public override int Order => 0;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ConfigMap, string>()
            {
                Name = "Keys",
                Display = x => x.Data?.Keys.Count > 0 ? x.Data.Keys.Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Data?.Keys.FirstOrDefault() ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }
}

