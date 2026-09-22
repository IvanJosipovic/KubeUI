using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Docking;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public interface IResourceEditorLauncher
{
    void Open(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource);
    void OpenNew(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource);
}

internal sealed class ResourceEditorLauncher : IResourceEditorLauncher
{
    private readonly IServiceProvider _services;
    private readonly Func<IFactory> _factory;

    public ResourceEditorLauncher(IServiceProvider services, Func<IFactory> factory)
    {
        _services = services;
        _factory = factory;
    }

    public void Open(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
        => OpenCore(cluster, resource, false);

    public void OpenNew(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
        => OpenCore(cluster, resource, true);

    private void OpenCore(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, bool isNew)
    {
        var vm = _services.GetRequiredService<ResourceEditorViewModel>();
        if (isNew)
            vm.InitializeNew(cluster, resource);
        else
            vm.Initialize(cluster, resource);
        var documents = _factory().GetDockable<IDocumentDock>("Documents")
            ?? throw new InvalidOperationException("Documents dock is not available.");
        var existing = documents.VisibleDockables?.OfType<ResourceEditorViewModel>()
            .FirstOrDefault(item => item.Id == vm.Id);
        if (existing is not null)
        {
            vm.Dispose();
            _factory().SetActiveDockable(existing);
            _factory().SetFocusedDockable(documents, existing);
            return;
        }
        _factory().AddToDocuments(vm);
    }
}
