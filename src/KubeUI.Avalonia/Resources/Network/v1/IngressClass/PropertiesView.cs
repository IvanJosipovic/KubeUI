using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Network.v1.IngressClass;

public sealed class PropertiesView : ViewBase<V1IngressClass>
{
    protected override object Build(V1IngressClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.IngressClassPropertiesView_Controller!)
                    .Value(vm.Spec?.Controller ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.IngressClassPropertiesView_Parameters_Kind!)
                    .Value(vm.Spec?.Parameters?.Kind ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.IngressClassPropertiesView_Parameters_Name!)
                    .Value(vm.Spec?.Parameters?.Name ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.IngressClassPropertiesView_Parameters_Scope!)
                    .Value(vm.Spec?.Parameters?.Scope ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.IngressClassPropertiesView_Parameters_Namespace!)
                    .Value(vm.Spec?.Parameters?.NamespaceProperty ?? ""));
    }
}
