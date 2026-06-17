using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding;

public sealed class PropertiesView : ViewBase<V1ClusterRoleBinding>
{
    protected override object Build(V1ClusterRoleBinding vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Role_Kind)
                    .Value(vm.RoleRef.Kind),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Role_Name)
                    .Value(vm.RoleRef.Name),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Subjects)
                    .Value(vm.Subjects.Count));
    }
}
