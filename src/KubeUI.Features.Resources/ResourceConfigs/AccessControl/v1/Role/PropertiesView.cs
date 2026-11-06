using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.AccessControl.v1.Role;

public sealed class PropertiesView : ViewBase<V1Role>
{
    protected override object Build(V1Role vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Rules)
                    .Value(vm.Rules.Count));
    }
}
