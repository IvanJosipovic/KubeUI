using Dock.Model.Core;
using Humanizer;
using k8s.Models;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources;

public sealed partial class V1CustomResourceDefinitionConfig : ResourceConfigBase<V1CustomResourceDefinition>
{
    public override int Order => 13;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Name",
                Display = x => x.Spec.Names.Kind.Humanize(LetterCasing.Title),
                Field = x => x.Spec.Names.Kind,
                Sort = SortDirection.Ascending,
                Width = "2*",
            },
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Group",
                Field = x => x.Spec.Group,
                Width = "*",
            },
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Version",
                Field = x => x.Spec.Versions.First(x => x.Storage).Name,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Scope",
                Field = x => x.Spec.Scope,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "View Items",
                CommandParameterPath = Utilities.PathBuilder<ResourceListViewModel<V1CustomResourceDefinition>>(x => x.SelectedItem.Value),
                CommandPath =  nameof(ListCRDCommand)
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanListCRD))]
    private void ListCRD(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage);

        var type = Cluster.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
        var resourceListType = typeof(ResourceListViewModel<>).MakeGenericType(type);

        var vm = Application.Current.GetRequiredService(resourceListType) as IDockable;

        if (vm is IInitializeCluster init)
        {
            init.Initialize(Cluster);
        }

        _factory.AddToDocuments(vm);
    }

    private bool CanListCRD(V1CustomResourceDefinition? crd)
    {
        if (crd != null)
        {
            var version = crd.Spec.Versions.First(x => x.Served && x.Storage);
            var type = Cluster.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);

            return Cluster.CanIAnyNamespace(type, Verb.List) && Cluster.CanIAnyNamespace(type, Verb.Watch);
        }

        return false;
    }
}
