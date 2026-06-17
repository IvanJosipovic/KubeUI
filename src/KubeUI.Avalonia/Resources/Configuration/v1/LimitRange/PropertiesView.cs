using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.LimitRange;

public sealed class PropertiesView : ViewBase<V1LimitRange>
{
    protected override object Build(V1LimitRange vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Limits)
                    .Value(vm.Spec.Limits.Count));
    }
}
