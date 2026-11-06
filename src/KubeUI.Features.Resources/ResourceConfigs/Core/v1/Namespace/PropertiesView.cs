using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Core.v1.Namespace;

public sealed class PropertiesView : ViewBase<V1Namespace>
{
    protected override object Build(V1Namespace vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Phase.CurrentValue()!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.NamespacePropertiesView_Finalizers.CurrentValue()!)
                    .Value(vm.Spec?.Finalizers?.Count ?? 0),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Conditions.CurrentValue()!)
                    .Value(vm.Status?.Conditions?.Count ?? 0));
    }
}
