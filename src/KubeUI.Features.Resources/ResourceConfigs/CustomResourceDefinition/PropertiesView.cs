using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.CustomResourceDefinition;

public sealed class PropertiesView : ViewBase<V1CustomResourceDefinition>
{
    protected override object Build(V1CustomResourceDefinition vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.CustomResourceDefinitionPropertiesView_Group!)
                    .Value(vm.Spec?.Group ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.CustomResourceDefinitionPropertiesView_Scope!)
                    .Value(vm.Spec?.Scope ?? ""),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Kind!)
                    .Value(vm.Spec?.Names?.Kind ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.CustomResourceDefinitionPropertiesView_Versions!)
                    .Value(vm.Spec?.Versions?.Count ?? 0),
                new ExpandableSection()
                    .Header(SharedAssets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.CustomResourceDefinitionPropertiesView_Accepted_Kind!)
                                    .Value(vm.Status?.AcceptedNames?.Kind ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.CustomResourceDefinitionPropertiesView_Stored_Versions!)
                                    .Value(vm.Status?.StoredVersions?.Count ?? 0))));
    }
}
