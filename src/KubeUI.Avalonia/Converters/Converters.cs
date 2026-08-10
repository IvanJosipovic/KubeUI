using System.Globalization;
using Avalonia.Data.Converters;

namespace KubeUI.Avalonia.Converters;

/// <summary>
/// Provides a set of useful <see cref="IValueConverter"/>s
/// </summary>
public static class Converters
{
    public static readonly IValueConverter NotNull = new FuncValueConverter<object?, bool>((x) => x != null && x != AvaloniaProperty.UnsetValue);

    public static readonly IValueConverter IsNull = new FuncValueConverter<object?, bool>((x) => x == null || x == AvaloniaProperty.UnsetValue);

    public static IValueConverter StringFormat(string Format)
    {
        return new FuncValueConverter<object?, string>(value =>
        {
            if (value != null && value != AvaloniaProperty.UnsetValue)
            {
                return string.Format(Format, value);
            }
            return string.Empty;
        });
    }
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
