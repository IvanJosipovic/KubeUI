using Dock.Model.Core;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Defines a custom document that can be shown under a navigation section.</summary>
/// <param name="Id">Stable document identifier, unique across sections.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position within its section.</param>
/// <param name="Icon">Optional icon.</param>
/// <param name="IsAvailable">Returns whether the document is available for a cluster.</param>
/// <param name="CreateDockable">Creates and initializes the document for a cluster.</param>
public sealed record NavigationDocument(
    string Id,
    string Name,
    int Order,
    Icon? Icon,
    Func<ClusterWorkspace, bool> IsAvailable,
    Func<IServiceProvider, ClusterWorkspace, IDockable> CreateDockable);
