using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Core.v1.Namespace.Views;

namespace KubeUI.Avalonia.Resources.Core.v1.Namespace;

public sealed partial class V1NamespaceConfig : ResourceConfigBase<V1Namespace>
{
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1Namespace, string>()
            {
                Name = "Labels",
                Field = x => x.Metadata.Labels?.Select(x => x.Key + "=" + x.Value).Aggregate((x,y) => x + ", " + y) ?? "",
                Width = "2*"
            },
            new ResourceListColumn<V1Namespace, string>()
            {
                Name = "Status",
                Field = x => x.Status?.Phase ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Namespace resource) => [new PropertiesView()];
}

