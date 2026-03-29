using Avalonia.Data.Converters;
using k8s;
using k8s.Models;
using System.Globalization;

namespace KubeUI.Avalonia.Converters;

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

public sealed class PropertyItemValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || value == AvaloniaProperty.UnsetValue)
        {
            return string.Empty;
        }

        if (value is DateTimeOffset dto)
        {
            return dto.ToLocalTime().ToString(culture);
        }

        if (value is DateTime dt)
        {
            return dt.Kind == DateTimeKind.Utc
                ? dt.ToLocalTime().ToString(culture)
                : dt.ToString(culture);
        }

        return value.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

