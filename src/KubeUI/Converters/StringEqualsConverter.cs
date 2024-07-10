using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace KubeUI.Converters;

public class StringEqualsConverter : IValueConverter
{
    public static readonly StringEqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sourceText && parameter is string equalsText && targetType.IsAssignableTo(typeof(bool)))
        {
            return sourceText.Equals(equalsText, StringComparison.InvariantCultureIgnoreCase);
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
