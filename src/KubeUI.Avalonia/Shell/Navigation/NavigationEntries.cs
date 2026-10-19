using FluentIcons.Common;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Desired state for one generated navigation link.</summary>
/// <param name="Id">Stable link identifier.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to siblings.</param>
/// <param name="Icon">Optional link icon.</param>
/// <param name="Section">Owning section, or null for a cluster-root link.</param>
/// <param name="Groups">Groups between the section root and this link.</param>
public abstract record NavigationEntry(
    string Id,
    string Name,
    int Order,
    Icon? Icon,
    NavigationSection? Section,
    IReadOnlyList<NavigationGroup> Groups);

/// <summary>A desired link that opens a resource list.</summary>
/// <param name="Id">Stable link identifier.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to siblings.</param>
/// <param name="Icon">Optional link icon.</param>
/// <param name="Section">Owning section, or null for a cluster-root link.</param>
/// <param name="Groups">Groups between the section root and this link.</param>
/// <param name="Kind">The resource kind the link opens.</param>
/// <param name="IsAlias">True when this link is a section alias rather than the canonical link.</param>
public sealed record ResourceEntry(
    string Id,
    string Name,
    int Order,
    Icon? Icon,
    NavigationSection? Section,
    IReadOnlyList<NavigationGroup> Groups,
    GroupApiVersionKind Kind,
    bool IsAlias) : NavigationEntry(Id, Name, Order, Icon, Section, Groups);

/// <summary>A desired link that opens a section-owned document.</summary>
/// <param name="Id">Stable link identifier.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to siblings.</param>
/// <param name="Icon">Optional link icon.</param>
/// <param name="Section">Owning section, or null for a cluster-root link.</param>
/// <param name="Groups">Groups between the section root and this link.</param>
/// <param name="DocumentId">The stable document identifier resolved through the section catalog.</param>
public sealed record DocumentEntry(
    string Id,
    string Name,
    int Order,
    Icon? Icon,
    NavigationSection? Section,
    IReadOnlyList<NavigationGroup> Groups,
    string DocumentId) : NavigationEntry(Id, Name, Order, Icon, Section, Groups);

/// <summary>A desired link that opens a built-in navigation target.</summary>
/// <param name="Id">Stable link identifier.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to siblings.</param>
/// <param name="Icon">Optional link icon.</param>
/// <param name="Section">Owning section, or null for a cluster-root link.</param>
/// <param name="Groups">Groups between the section root and this link.</param>
/// <param name="ViewModelKey">The built-in target key resolved by the selection handler.</param>
public sealed record BuiltinEntry(
    string Id,
    string Name,
    int Order,
    Icon? Icon,
    NavigationSection? Section,
    IReadOnlyList<NavigationGroup> Groups,
    string ViewModelKey) : NavigationEntry(Id, Name, Order, Icon, Section, Groups);
