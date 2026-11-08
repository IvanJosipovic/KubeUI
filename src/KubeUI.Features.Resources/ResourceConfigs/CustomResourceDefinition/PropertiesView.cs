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
                    .Key(L10n.Instance.CustomResourceDefinitionPropertiesView_Group.CurrentValue()!)
                    .Value(vm.Spec?.Group ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.CustomResourceDefinitionPropertiesView_Scope.CurrentValue()!)
                    .Value(vm.Spec?.Scope ?? ""),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Kind.CurrentValue()!)
                    .Value(vm.Spec?.Names?.Kind ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.CustomResourceDefinitionPropertiesView_Versions.CurrentValue()!)
                    .Value(vm.Spec?.Versions?.Count ?? 0),
                new ExpandableSection()
                    .Header(SharedL10n.Instance.Shared_Status.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.CustomResourceDefinitionPropertiesView_Accepted_Kind.CurrentValue()!)
                                    .Value(vm.Status?.AcceptedNames?.Kind ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.CustomResourceDefinitionPropertiesView_Stored_Versions.CurrentValue()!)
                                    .Value(vm.Status?.StoredVersions?.Count ?? 0))));
    }
}
