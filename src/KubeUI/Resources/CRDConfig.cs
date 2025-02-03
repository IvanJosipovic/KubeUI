using Dock.Model.Core;
using Humanizer;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1CustomResourceDefinition>>(ServiceLifetime.Transient)]
public sealed partial class CRDConfig : ResourceConfigBase<V1CustomResourceDefinition>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override bool ShowNamespaces => false;
    public override int Order => 13;

    public CRDConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Name",
                Display = x => x.Spec.Names.Kind.Humanize(LetterCasing.Title),
                Field = x => x.Spec.Names.Kind,
                Sort = SortDirection.Ascending,
                Width = "2*",
            },
            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Group",
                Field = x => x.Spec.Group,
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Version",
                Field = x => x.Spec.Versions.First(x => x.Storage).Name,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Scope",
                Field = x => x.Spec.Scope,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "View Items",
                CommandParameterPath = "SelectedItem.Value",
                CommandPath = nameof(ResourceListViewModel<V1Pod>.ListCRDCommand)
            },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override object? Properties(V1CustomResourceDefinition resource)
    {
        return null;
    }
}
