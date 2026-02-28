using Avalonia.Data.Converters;
using k8s;
using k8s.Models;

namespace KubeUI.Converters;

/// <summary>
/// Provides a set of useful <see cref="IValueConverter"/>s
/// </summary>
public static class Converters
{
    /// <summary>
    /// A value converter that returns the Controller Name if value is a IKubernetesObject<V1ObjectMeta>
    /// </summary>
    public static readonly IValueConverter ObjectOwnerName =
        new FuncValueConverter<object, string>(value =>
        {
            if (value is IKubernetesObject<V1ObjectMeta> obj)
            {
                return obj.Metadata.OwnerReferences?.FirstOrDefault(x => x.Controller == true)?.Name ?? "N/A";
            }

            return "N/A";
        });

    public static readonly IValueConverter NotNull = new FuncValueConverter<object, bool>((x) => x != null && x != AvaloniaProperty.UnsetValue);
}
