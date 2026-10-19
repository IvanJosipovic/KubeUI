namespace KubeUI.Avalonia.Resources;

/// <summary>Shared helpers over <see cref="IResourceConfig"/>.</summary>
public static class ResourceConfigExtensions
{
    /// <summary>Gets whether a resource configuration is backed by a custom resource definition.</summary>
    /// <param name="resource">The resource configuration to inspect.</param>
    public static bool IsCustomResource(this IResourceConfig resource)
        => resource.Origin == ResourceOrigin.CustomResourceDefinition;
}
