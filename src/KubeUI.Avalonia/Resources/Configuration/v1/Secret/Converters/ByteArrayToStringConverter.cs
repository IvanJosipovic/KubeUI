using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Secret.Converters;

public sealed class ByteArrayToStringConverter : IValueConverter
{
    public static readonly ByteArrayToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] bytes)
            return Encoding.UTF8.GetString(bytes);

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            return Encoding.UTF8.GetBytes(s);

        return Array.Empty<byte>();
    }
}

