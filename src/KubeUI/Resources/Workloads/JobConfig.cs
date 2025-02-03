using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Job>>(ServiceLifetime.Transient)]
public sealed partial class JobConfig : ResourceConfigBase<V1Job>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;
    public override string Category => "Workloads";
    public override int Order => 5;

    public JobConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1Job, int>()
            {
                Name = "Completions",
                Display = x => $"{x.Status.Succeeded ?? 0}/{x.Spec.Completions ?? 0}",
                Field = x => x.Spec.Completions ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            new ResourceListViewDefinitionColumn<V1Job, string>()
            {
                Name = "Conditions",
                Field = x => x.Status?.Conditions?.FirstOrDefault(y => y.Status == "True")?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override Control[]? Properties(V1Job resource)
    {
        return null;
    }
}
