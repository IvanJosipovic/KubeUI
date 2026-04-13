using System.Globalization;
using Avalonia.Data.Converters;

namespace KubeUI.Avalonia.Shell.Documents.Settings.Views;

public sealed partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
}

// Converter placed in backing file as requested.
internal sealed class ThemeEqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null && parameter != null && value.Equals(parameter);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b && parameter is not null) ? parameter : BindingOperations.DoNothing;
}

