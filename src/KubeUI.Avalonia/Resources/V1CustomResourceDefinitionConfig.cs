using Dock.Model.Core;
using k8s.Models;

namespace KubeUI.Avalonia.Resources;

public sealed partial class V1CustomResourceDefinitionConfig : ResourceConfigBase<V1CustomResourceDefinition>
{
    public override int Order => 13;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Name = "Name",
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

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1CustomResourceDefinition>? selectedItems)
    {
        var selectedItem = selectedItems?.FirstOrDefault();

        return [
            new()
            {
                Header = "View Items",
                Command = ListCRDCommand,
                CommandParameter = selectedItem
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanListCRD))]
    private void ListCRD(V1CustomResourceDefinition crd)
    {
        var spec = crd.Spec;
        if (spec == null)
        {
            return;
        }

        var version = spec.Versions?.FirstOrDefault(x => x.Storage);
        if (version == null)
        {
            return;
        }

        var type = Cluster.ModelCache.GetResourceType(spec.Group, version.Name, spec.Names.Kind);
        if (type == null)
        {
            return;
        }

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
        if (crd == null || crd.Spec == null)
        {
            return false;
        }

        var spec = crd.Spec;
        var version = spec.Versions?.FirstOrDefault(x => x.Served && x.Storage);
        if (version == null)
        {
            return false;
        }

        var type = Cluster.ModelCache.GetResourceType(spec.Group, version.Name, spec.Names.Kind);
        if (type == null)
        {
            return false;
        }

        return Cluster.CanIAnyNamespace(type, Verb.List) && Cluster.CanIAnyNamespace(type, Verb.Watch);
    }
}


