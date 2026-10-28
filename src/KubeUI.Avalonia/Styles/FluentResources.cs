namespace KubeUI.Avalonia.Styles;

internal static class FluentResources
{
    internal static T GetResource<T>(Application application, object key, ThemeVariant themeVariant)
        where T : class
    {
        if (!application.TryGetResource(key, themeVariant, out var resource) || resource is not T typed)
        {
            throw new InvalidOperationException($"Required Fluent resource '{key}' was not found.");
        }

        return typed;
    }
}
