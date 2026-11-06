using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.ValidatingWebhookConfiguration;

public sealed class PropertiesView : ViewBase<V1ValidatingWebhookConfiguration>
{
    protected override object Build(V1ValidatingWebhookConfiguration vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Webhooks.CurrentValue()!)
                    .Value(vm.Webhooks.Count));
    }
}
