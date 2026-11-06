using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Network.v1.IngressClass;

public sealed class PropertiesView : ViewBase<V1IngressClass>
{
    protected override object Build(V1IngressClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.IngressClassPropertiesView_Controller.CurrentValue()!)
                    .Value(vm.Spec?.Controller ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.IngressClassPropertiesView_Parameters_Kind.CurrentValue()!)
                    .Value(vm.Spec?.Parameters?.Kind ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.IngressClassPropertiesView_Parameters_Name.CurrentValue()!)
                    .Value(vm.Spec?.Parameters?.Name ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.IngressClassPropertiesView_Parameters_Scope.CurrentValue()!)
                    .Value(vm.Spec?.Parameters?.Scope ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.IngressClassPropertiesView_Parameters_Namespace.CurrentValue()!)
                    .Value(vm.Spec?.Parameters?.NamespaceProperty ?? ""));
    }
}
