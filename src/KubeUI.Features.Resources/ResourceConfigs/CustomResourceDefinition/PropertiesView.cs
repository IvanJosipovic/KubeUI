using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Properties;

namespace KubeUI.Features.Resources.CustomResourceDefinition;

public sealed class PropertiesView : ViewBase<V1CustomResourceDefinition>
{
    protected override object Build(V1CustomResourceDefinition vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.CustomResourceDefinitionPropertiesView_Group.CurrentValue()!)
                    .Value(vm.Spec?.Group ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.CustomResourceDefinitionPropertiesView_Scope.CurrentValue()!)
                    .Value(vm.Spec?.Scope ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Kind.CurrentValue()!)
                    .Value(vm.Spec?.Names?.Kind ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.CustomResourceDefinitionPropertiesView_Versions.CurrentValue()!)
                    .Value(vm.Spec?.Versions?.Count ?? 0),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Status,
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.CustomResourceDefinitionPropertiesView_Accepted_Kind.CurrentValue()!)
                        .Value(vm.Status?.AcceptedNames?.Kind ?? ""),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.CustomResourceDefinitionPropertiesView_Stored_Versions.CurrentValue()!)
                        .Value(vm.Status?.StoredVersions?.Count ?? 0)));
    }
}
