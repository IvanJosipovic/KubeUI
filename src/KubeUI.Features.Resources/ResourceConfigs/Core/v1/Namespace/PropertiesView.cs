using k8s.Models;
using KubeUI.Features.Resources.Metrics.Controls;
using KubeUI.Features.Resources.Properties.Controls;

#pragma warning disable CA1716 // Public namespace is an API contract; renaming would be a breaking change.
namespace KubeUI.Features.Resources.Core.v1.Namespace;
#pragma warning restore CA1716

public sealed class PropertiesView : ViewBase<V1Namespace>
{
    protected override object Build(V1Namespace vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(new MetricsControl { DataContext = vm },
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Phase.CurrentValue()!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.NamespacePropertiesView_Finalizers.CurrentValue()!)
                    .Value(vm.Spec?.Finalizers?.Count ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                    .Value(vm.Status?.Conditions?.Count ?? 0));
    }
}
