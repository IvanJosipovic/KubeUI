using System.Reflection;

namespace KubeUI.Avalonia.Infrastructure;

internal static class ResourceStrings
{
    public static string? GetString(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return typeof(Assets.Resources).GetProperty(name, BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
    }
}
