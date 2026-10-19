using Dock.Model.Core;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Context used to decide whether a section-owned document is available.</summary>
/// <param name="Cluster">Workspace whose permissions and resources determine availability.</param>
public sealed record NavigationDocumentContext(ClusterWorkspace Cluster);

/// <summary>Defines a custom document that can be shown under a navigation section.</summary>
public interface INavigationDocument
{
    /// <summary>Gets the stable document identifier.</summary>
    string Id { get; }

    /// <summary>Gets the localized display name.</summary>
    string Name { get; }

    /// <summary>Gets the sort position within its section.</summary>
    int Order { get; }

    /// <summary>Gets the optional icon.</summary>
    Icon? Icon { get; }

    /// <summary>Returns whether this document is available for the given cluster.</summary>
    bool IsAvailable(NavigationDocumentContext context);

    /// <summary>Creates and initializes this document.</summary>
    IDockable CreateDockable(IServiceProvider services, ClusterWorkspace cluster);
}

/// <summary>Defines a section-owned document that resolves its view model from dependency injection.</summary>
/// <typeparam name="TViewModel">The dockable view model type.</typeparam>
public sealed class NavigationDocument<TViewModel> : INavigationDocument
    where TViewModel : class, IDockable
{
    private readonly Func<NavigationDocumentContext, bool> _isAvailable;

    /// <summary>Defines a document that resolves its view model from dependency injection.</summary>
    /// <param name="id">Stable document identifier.</param>
    /// <param name="name">Localized display name.</param>
    /// <param name="order">Sort position within its section.</param>
    /// <param name="icon">Optional icon.</param>
    /// <param name="isAvailable">Checks whether the document is available for a cluster.</param>
    public NavigationDocument(
        string id,
        string name,
        int order,
        Icon? icon,
        Func<NavigationDocumentContext, bool> isAvailable)
    {
        Id = id;
        Name = name;
        Order = order;
        Icon = icon;
        _isAvailable = isAvailable;
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public int Order { get; }

    /// <inheritdoc />
    public Icon? Icon { get; }

    /// <inheritdoc />
    public bool IsAvailable(NavigationDocumentContext context) => _isAvailable(context);

    /// <inheritdoc />
    public IDockable CreateDockable(IServiceProvider services, ClusterWorkspace cluster)
    {
        var viewModel = services.GetRequiredService<TViewModel>();
        if (viewModel is IInitializeCluster initializeCluster)
        {
            initializeCluster.Initialize(cluster);
        }

        return viewModel;
    }
}
