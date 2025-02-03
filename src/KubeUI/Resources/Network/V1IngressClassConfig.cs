using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1IngressClass>>(ServiceLifetime.Transient)]
public sealed partial class V1IngressClassConfig : ResourceConfigBase<V1IngressClass>
{
    public override string Category => "Network";
    public override int Order => 4;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
            {
                Name = "Controller",
                Field = x => x.Spec.Controller,
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
            {
                Name = "API Group",
                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.ApiGroup : "",
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
            {
                Name = "Scope",
                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Scope : "",
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1IngressClass, string>()
            {
                Name = "Kind",
                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Kind : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[]? Properties(V1IngressClass resource)
    {
        return null;
    }
}
