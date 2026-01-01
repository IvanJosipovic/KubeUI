using System.Globalization;
using Avalonia.Data.Converters;

namespace KubeUI.Converters;

class StaticResourceConverter : IValueConverter
{
    public static readonly StaticResourceConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        return Application.Current.FindResource(value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new Exception("The method or operation is not implemented.");
    }
}
