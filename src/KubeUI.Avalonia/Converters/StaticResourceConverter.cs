using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace KubeUI.Avalonia.Converters;

class StaticResourceConverter : IValueConverter
{
    public static readonly StaticResourceConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        return Application.Current?.FindResource(value) ?? AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new Exception("The method or operation is not implemented.");
    }
}

