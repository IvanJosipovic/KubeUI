using k8s.Models;

namespace KubeUI.Features.Resources.Network.v1.IngressClass;

public sealed partial class V1IngressClassConfig : ResourceConfig<V1IngressClass>
{
    public V1IngressClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.NetworkId);
    public override int Order => 4;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "controller",
                Name = ResourcesLanguageManager.Instance.V1IngressClassConfig_Controller.CurrentValue()!,
                Field = x => x.Spec.Controller,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "api-group",
                Name = ResourcesLanguageManager.Instance.V1IngressClassConfig_API_Group.CurrentValue()!,
                Field = x => x.Spec?.Parameters?.ApiGroup ?? "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "scope",
                Name = ResourcesLanguageManager.Instance.V1IngressClassConfig_Scope.CurrentValue()!,
                Field = x => x.Spec.Parameters?.Scope ?? "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "kind",
                Name = ResourcesLanguageManager.Instance.V1IngressClassConfig_Kind.CurrentValue()!,
                Field = x => x.Spec.Parameters?.Kind ?? "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1IngressClass resource) => [new PropertiesView()];
}
