using k8s.Models;

namespace KubeUI.Features.Resources.Core.v1.Namespace;

public sealed partial class V1NamespaceConfig(IServiceProvider serviceProvider) : ResourceConfig<V1Namespace>(serviceProvider)
{
    public override int Order => 6;
    public override bool SeedOnConnect => true;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1Namespace, string>()
            {
                Key = "labels",
                Name = L10n.Instance.V1NamespaceConfig_Labels.CurrentValue()!,
                Field = x => x?.Metadata?.Labels is { Count: > 0 } labels ? string.Join(", ", labels.Select(x => x.Key + "=" + x.Value)) : "",
                Width = 2, WidthMode = DynamicTableViewWidthMode.Star
            },
            new ResourceListColumn<V1Namespace, string>()
            {
                Key = "status",
                Name = L10n.Instance.V1NamespaceConfig_Status.CurrentValue()!,
                Field = x => x?.Status?.Phase ?? "",
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Namespace resource) => [new PropertiesView()];
}
