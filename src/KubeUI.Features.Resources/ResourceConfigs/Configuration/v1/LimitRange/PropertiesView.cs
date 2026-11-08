using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.LimitRange;

public sealed class PropertiesView : ViewBase<V1LimitRange>
{
    protected override object Build(V1LimitRange vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Limits.CurrentValue()!)
                    .Value(vm.Spec.Limits.Count));
    }
}
