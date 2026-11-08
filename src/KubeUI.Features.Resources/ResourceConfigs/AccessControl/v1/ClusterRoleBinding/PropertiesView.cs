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
                    .Key(FoundationLanguageManager.Instance.Shared_Role_Kind.CurrentValue()!)
                    .Value(vm.RoleRef.Kind),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Role_Name.CurrentValue()!)
                    .Value(vm.RoleRef.Name),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Subjects.CurrentValue()!)
                    .Value(vm.Subjects.Count));
    }
}
