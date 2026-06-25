using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Network.v1.Ingress;

public sealed partial class V1IngressConfig : ResourceConfigBase<V1Ingress>
{
    public V1IngressConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Network!;
    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Ingress, string>()
            {
                Key = "load-balancers",
                Name = Assets.Resources.V1IngressConfig_Load_Balancers!,
                Field = x => x.Status?.LoadBalancer?.Ingress is { Count: > 0 } ingress ? string.Join(", ", ingress.Select(x => x.Ip ?? x.Hostname).Where(x => !string.IsNullOrEmpty(x))) : "",
                Width = "*",
            },
            new ResourceListColumn<V1Ingress, string>()
            {
                Key = "rules",
                Name = Assets.Resources.V1IngressConfig_Rules!,
                Field = x => x.Spec?.Rules is { Count: > 0 } rules ? string.Join(", ", rules.Select(x => x.Http?.Paths is { Count: > 0 } paths ? $"http://{x.Host}{paths[0].Path}" : $"http://{x.Host}")) : "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Ingress resource) => [new PropertiesView()];
}
