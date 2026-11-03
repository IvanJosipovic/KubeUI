using System.Globalization;
using Avalonia.Data.Converters;

namespace KubeUI.Controls.Converters;

/// <summary>Resolves a resource key from the current Avalonia application resources.</summary>
public class StaticResourceConverter : IValueConverter
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

