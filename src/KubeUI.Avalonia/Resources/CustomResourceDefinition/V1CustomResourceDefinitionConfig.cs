using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Resources.CustomResourceDefinition;

public sealed partial class V1CustomResourceDefinitionConfig : ResourceConfigBase<V1CustomResourceDefinition>
{
    public override bool SeedOnConnect => true;
    public V1CustomResourceDefinitionConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override int Order => 13;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Key = "name",
                Name = Assets.Resources.V1CustomResourceDefinitionConfig_Name!,
                Field = x => x.Spec.Names.Kind,
                Sort = SortDirection.Ascending,
                Width = "2*",
            },
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Key = "group",
                Name = Assets.Resources.V1CustomResourceDefinitionConfig_Group!,
                Field = x => x.Spec.Group,
                Width = "*",
            },
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Key = "version",
                Name = Assets.Resources.V1CustomResourceDefinitionConfig_Version!,
                Field = x => x.Spec.Versions.First(x => x.Storage).Name,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1CustomResourceDefinition, string>()
            {
                Key = "scope",
                Name = Assets.Resources.V1CustomResourceDefinitionConfig_Scope!,
                Field = x => x.Spec.Scope,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1CustomResourceDefinition resource) => [new PropertiesView()];

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1CustomResourceDefinition>? selectedItems)
    {
        var selectedItem = selectedItems?.FirstOrDefault();

        return [
            new()
            {
                Title = Assets.Resources.V1CustomResourceDefinitionConfig_MenuItem_ViewItems,
                FluentIcon = Icon.AppsList,
                Command = ListCRDCommand,
                CommandParameter = selectedItem
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanListCRD))]
    private void ListCRD(V1CustomResourceDefinition crd)
    {
        if (!crd.TryGetResourceKind(out var kind))
        {
            return;
        }
        if (!Cluster.Runtime.ModelCatalog.IsCustomResource(kind))
        {
            return;
        }

        var vm = ServiceProvider.GetRequiredService<ResourceListViewModel<GenericKubernetesObject>>();
        vm.InitializeResource(Cluster, kind);

        _factory.AddToDocuments(vm);
    }

    private bool CanListCRD(V1CustomResourceDefinition? crd)
    {
        if (crd == null || crd.Spec == null)
        {
            return false;
        }

        if (!crd.TryGetResourceKind(out var kind))
        {
            return false;
        }
        if (!Cluster.Runtime.ModelCatalog.IsCustomResource(kind))
        {
            return false;
        }

        var resourceConfig = Cluster.GetResourceConfig(kind);
        return resourceConfig?.PermissionsLoaded == true && resourceConfig.CanListAndWatch;
    }
}
