using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Resources;

/// <summary>Owns a top-level navigation section and the resources and documents shown in it.</summary>
public interface INavigationSectionProvider
{
    /// <summary>Gets the section this provider owns.</summary>
    NavigationSection Section { get; }

    /// <summary>Gets this section's alias placement for a resource kind, or null when unmatched.</summary>
    /// <param name="resource">The resource configuration being considered.</param>
    NavigationPlacement? Place(IResourceConfig resource);

    /// <summary>Gets non-resource documents owned by this section.</summary>
    IReadOnlyList<INavigationDocument> Documents { get; }
}
