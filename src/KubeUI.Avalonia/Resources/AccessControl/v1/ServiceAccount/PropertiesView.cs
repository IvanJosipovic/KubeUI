using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ServiceAccount;

public sealed class PropertiesView : ViewBase<V1ServiceAccount>
{
    protected override object Build(V1ServiceAccount vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.ServiceAccountPropertiesView_Automount_Token)
                    .Value(vm.AutomountServiceAccountToken),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceAccountPropertiesView_Image_Pull_Secrets)
                    .Value(vm.ImagePullSecrets.Count),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Secrets)
                    .Value(vm.Secrets.Count));
    }
}
