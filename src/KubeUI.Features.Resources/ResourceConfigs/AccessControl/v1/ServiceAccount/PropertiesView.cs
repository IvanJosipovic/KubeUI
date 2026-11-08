using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.AccessControl.v1.ServiceAccount;

public sealed class PropertiesView : ViewBase<V1ServiceAccount>
{
    protected override object Build(V1ServiceAccount vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.ServiceAccountPropertiesView_Automount_Token.CurrentValue()!)
                    .Value(vm.AutomountServiceAccountToken),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.ServiceAccountPropertiesView_Image_Pull_Secrets.CurrentValue()!)
                    .Value(vm.ImagePullSecrets.Count),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Secrets.CurrentValue()!)
                    .Value(vm.Secrets.Count));
    }
}
