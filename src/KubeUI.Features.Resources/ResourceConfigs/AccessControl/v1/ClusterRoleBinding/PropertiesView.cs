using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.AccessControl.v1.ClusterRoleBinding;

public sealed class PropertiesView : ViewBase<V1ClusterRoleBinding>
{
    protected override object Build(V1ClusterRoleBinding vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Role_Kind)
                    .Value(vm.RoleRef.Kind),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Role_Name)
                    .Value(vm.RoleRef.Name),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Subjects)
                    .Value(vm.Subjects.Count));
    }
}
