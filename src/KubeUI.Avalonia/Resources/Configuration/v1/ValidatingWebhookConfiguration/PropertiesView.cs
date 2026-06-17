using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ValidatingWebhookConfiguration;

public sealed class PropertiesView : ViewBase<V1ValidatingWebhookConfiguration>
{
    protected override object Build(V1ValidatingWebhookConfiguration vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Webhooks)
                    .Value(vm.Webhooks.Count));
    }
}
