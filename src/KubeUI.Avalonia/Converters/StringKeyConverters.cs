using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace KubeUI.Avalonia.Converters
{
    public sealed class StringNotNullOrEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || value == AvaloniaProperty.UnsetValue)
                return false;

            return !string.IsNullOrWhiteSpace(value.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public sealed class StringNullOrWhiteSpaceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || value == AvaloniaProperty.UnsetValue)
                return true;

            return string.IsNullOrWhiteSpace(value.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
