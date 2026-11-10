using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.MutatingWebhookConfiguration;

public sealed class PropertiesView : ViewBase<V1MutatingWebhookConfiguration>
{
    protected override object Build(V1MutatingWebhookConfiguration vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Webhooks.CurrentValue()!)
                    .Value(vm.Webhooks.Count));
    }
}
