using Avalonia.Data.Converters;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

internal static class ResourcePropertyBindingExtensions
{
    internal static PropertyItem BindValue<TResource>(
        this PropertyItem item,
        ResourcePropertiesViewModel<TResource> viewModel,
        Func<TResource?, object?> valueSelector)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(valueSelector);

        item.Bind(
            PropertyItem.ValueProperty,
            new Binding(nameof(ResourcePropertiesViewModel<TResource>.Object))
            {
                Source = viewModel,
                Mode = BindingMode.OneWay,
                Converter = new FuncValueConverter<TResource?, object?>(valueSelector),
            });
        return item;
    }

    internal static CollectionItem BindValue<TResource>(
        this CollectionItem item,
        ResourcePropertiesViewModel<TResource> viewModel,
        Func<TResource?, IEnumerable?> valueSelector)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(valueSelector);

        // CollectionItem.Value requires IEnumerable, so project Object with a converter.
        item.Bind(
            CollectionItem.ValueProperty,
            new Binding(nameof(ResourcePropertiesViewModel<TResource>.Object))
            {
                Source = viewModel,
                Mode = BindingMode.OneWay,
                Converter = new FuncValueConverter<TResource?, IEnumerable>(
                    resource => valueSelector(resource) ?? Array.Empty<object>()),
            });
        return item;
    }
}
