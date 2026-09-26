using System.Globalization;
using Avalonia.Data.Converters;
using KubeUI.Avalonia.Features.Resources.Common;

namespace KubeUI.Avalonia.Features.Resources.List;

internal sealed class ResourceMenuItemIconConverter : IValueConverter
{
    public static ResourceMenuItemIconConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MenuItemViewModel item ? ResourceActionPresenter.CreateIcon(item) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
