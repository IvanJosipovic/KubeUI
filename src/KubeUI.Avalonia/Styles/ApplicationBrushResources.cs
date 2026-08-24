namespace KubeUI.Avalonia.Styles;

internal static class ApplicationBrushResources
{
    public static IBrush GetBrush(string resourceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);

        if (Application.Current?.TryGetResource(
                resourceKey,
                Application.Current.ActualThemeVariant,
                out var resource) != true)
        {
            throw new InvalidOperationException($"The brush resource '{resourceKey}' was not found.");
        }

        return resource switch
        {
            IBrush brush => brush,
            Color color => new SolidColorBrush(color),
            _ => throw new InvalidOperationException($"The resource '{resourceKey}' is not a brush or color.")
        };
    }
}
