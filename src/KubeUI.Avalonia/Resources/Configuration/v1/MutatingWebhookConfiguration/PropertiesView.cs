using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.MutatingWebhookConfiguration;

public sealed class PropertiesView : ViewBase<V1MutatingWebhookConfiguration>
{
    protected override object Build(V1MutatingWebhookConfiguration vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Webhooks)
                    .Value(vm.Webhooks.Count));
    }
}
