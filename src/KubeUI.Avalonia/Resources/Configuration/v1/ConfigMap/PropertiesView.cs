using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap;

public sealed class PropertiesView : ViewBase<V1ConfigMap>, IResourcePropertiesRefreshable<V1ConfigMap>
{
    private DataDisplay<V1ConfigMap, string>? _dataDisplay;
    private PropertyItem? _binaryDataProperty;
    private PropertyItem? _immutableProperty;

    protected override object Build(V1ConfigMap vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        _dataDisplay = new DataDisplay<V1ConfigMap, string>(
            vm,
            GroupApiVersionKind.From<V1ConfigMap>(),
            static resource => resource.Data,
            static value => value,
            static value => value);
        _binaryDataProperty = new PropertyItem()
            .Key(Assets.Resources.ConfigMapPropertiesView_Binary_Data)
            .Value(vm.BinaryData?.Count ?? 0);
        _immutableProperty = new PropertyItem()
            .Key(Assets.Resources.ConfigMapPropertiesView_Immutable)
            .Value(vm.Immutable);

        return new StackPanel()
            .Children(
                _binaryDataProperty,
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Data)
                    .IsExpanded(true)
                    .Content(_dataDisplay),
                _immutableProperty);
    }

    public void Refresh(V1ConfigMap resource)
    {
        _dataDisplay?.Refresh(resource);
        if (_binaryDataProperty is not null)
        {
            _binaryDataProperty.Value = resource.BinaryData?.Count ?? 0;
        }

        if (_immutableProperty is not null)
        {
            _immutableProperty.Value = resource.Immutable;
        }
    }
}
