namespace KubeUI.Controls.Styles;

/// <summary>Looks up the application brush resource for the active theme.</summary>
public static class ApplicationBrushResources
{
    /// <summary>Gets the brush or color resource with the requested key.</summary>
    /// <param name="resourceKey">Resource key to resolve.</param>
    /// <returns>The brush value for the active theme.</returns>
    /// <exception cref="InvalidOperationException">The resource is missing or is not a brush or color.</exception>
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
