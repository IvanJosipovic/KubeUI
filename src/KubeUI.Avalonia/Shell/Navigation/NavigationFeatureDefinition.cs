using Dock.Model.Core;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Shell.Navigation;

public enum NavigationFeaturePlacement
{
    CustomResourceDefinitions
}

public sealed record NavigationFeatureContext(ClusterWorkspace Cluster);

public interface INavigationFeatureDefinition
{
    string Id { get; }
    string Name { get; }
    string[] Path { get; }
    NavigationFeaturePlacement Placement { get; }
    int Order { get; }
    Icon? Icon { get; }
    bool IsAvailable(NavigationFeatureContext context);
    IDockable CreateDockable(IServiceProvider services, ClusterWorkspace cluster);
}

public sealed class NavigationFeatureDefinition<TViewModel> : INavigationFeatureDefinition
    where TViewModel : class, IDockable
{
    private readonly Func<NavigationFeatureContext, bool> _isAvailable;

    public NavigationFeatureDefinition(
        string id,
        string name,
        string[] path,
        NavigationFeaturePlacement placement,
        int order,
        Icon? icon,
        Func<NavigationFeatureContext, bool> isAvailable)
    {
        Id = id;
        Name = name;
        Path = path;
        Placement = placement;
        Order = order;
        Icon = icon;
        _isAvailable = isAvailable;
    }

    public string Id { get; }
    public string Name { get; }
    public string[] Path { get; }
    public NavigationFeaturePlacement Placement { get; }
    public int Order { get; }
    public Icon? Icon { get; }

    public bool IsAvailable(NavigationFeatureContext context) => _isAvailable(context);

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

public sealed class NavigationFeatureCatalog
{
    private readonly IReadOnlyDictionary<string, INavigationFeatureDefinition> _definitions;

    public NavigationFeatureCatalog(IEnumerable<INavigationFeatureDefinition> definitions)
    {
        _definitions = definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    }

    public IReadOnlyCollection<INavigationFeatureDefinition> Definitions => _definitions.Values.ToArray();

    public bool TryGet(string id, out INavigationFeatureDefinition definition) =>
        _definitions.TryGetValue(id, out definition!);
}
