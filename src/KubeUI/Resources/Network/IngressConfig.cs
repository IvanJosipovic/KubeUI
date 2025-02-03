
using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1Ingress>>(ServiceLifetime.Transient)]
public sealed partial class IngressConfig : ResourceConfigBase<V1Ingress>
{
    public override string Category => "Network";
    public override int Order => 3;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1Ingress, string>()
            {
                Name = "Load Balancers",
                Display = x => x.Status.LoadBalancer.Ingress.Select(x => x.Ip).Aggregate((a,b) => a + ", " + b),
                Field = x => x.Status.LoadBalancer.Ingress.Count > 0 ? x.Status.LoadBalancer.Ingress[0].Ip : "",
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1Ingress, string>()
            {
                Name = "Rules",
                Display = x => x.Spec.Rules.Select(z => $"http://{z.Host}{z.Http.Paths[0].Path}").Aggregate((a,b) => a + ", " + b),
                Field = x => x.Spec.Rules.Count > 0 ? x.Spec.Rules[0].Host : "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[]? Properties(V1Ingress resource)
    {
        return null;
    }
}
