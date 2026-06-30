using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Core.v1.Namespace;

public sealed partial class V1NamespaceConfig : ResourceConfigBase<V1Namespace>
{
    public V1NamespaceConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1Namespace, string>()
            {
                Key = "labels",
                Name = Assets.Resources.V1NamespaceConfig_Labels!,
                Field = x => x.Metadata?.Labels is { Count: > 0 } labels ? string.Join(", ", labels.Select(x => x.Key + "=" + x.Value)) : "",
                Width = "2*"
            },
            new ResourceListColumn<V1Namespace, string>()
            {
                Key = "status",
                Name = Assets.Resources.V1NamespaceConfig_Status!,
                Field = x => x.Status?.Phase ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Namespace resource) => [new PropertiesView()];
}
