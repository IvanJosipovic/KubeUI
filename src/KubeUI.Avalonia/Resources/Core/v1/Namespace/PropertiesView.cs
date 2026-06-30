using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Core.v1.Namespace;

public sealed class PropertiesView : ViewBase<V1Namespace>
{
    protected override object Build(V1Namespace vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Phase!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NamespacePropertiesView_Finalizers!)
                    .Value(vm.Spec?.Finalizers?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Conditions!)
                    .Value(vm.Status?.Conditions?.Count ?? 0));
    }
}
