using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Docking;

namespace KubeUI.Avalonia.Features.Resources.Properties;

internal sealed class ResourcePropertiesCoordinator(IFactory factory, IServiceProvider services)
{
    public void Open<T>(ClusterWorkspace cluster, T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
        => factory.AddToRight(Create(cluster, resource));

    public void Follow<T>(ClusterWorkspace cluster, T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        const string id = nameof(ResourcePropertiesViewModel<>);
        var pinned = factory.FindRightPinnedDockable(id);
        var open = pinned ?? factory.FindDockableById(id);
        if (open is null)
            return;

        if (open is ResourcePropertiesViewModel<T> properties)
        {
            if (!ReferenceEquals(properties.Object, resource) || !ReferenceEquals(properties.Cluster, cluster))
                properties.ShowResource(cluster, resource);
            return;
        }

        var replacement = Create(cluster, resource);
        if (pinned is not null || !factory.ReplaceDockable(open, replacement))
            factory.AddToRight(replacement, previewPinned: false, focus: false);
    }

    private ResourcePropertiesViewModel<T> Create<T>(ClusterWorkspace cluster, T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var instance = services.GetRequiredService<ResourcePropertiesViewModel<T>>();
        instance.Initialize(cluster, resource);
        instance.CanFloat = false;
        instance.CanDockAsDocument = false;
        return instance;
    }
}
