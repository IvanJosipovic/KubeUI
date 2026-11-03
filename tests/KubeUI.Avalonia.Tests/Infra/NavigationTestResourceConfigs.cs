using System.Collections;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Common;
using KubeUI.Features.Resources.List;
using KubeUI.Features.Resources;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Tests.Infra;

internal sealed class BlockingPodPermissionResourceConfig : IResourceConfig
{
    private readonly Task _releaseTask;

    public BlockingPodPermissionResourceConfig(Task releaseTask) => _releaseTask = releaseTask;

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; private set; }
    public bool PermissionsLoaded { get; private set; }
    public bool ShowNewResource => true;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public NavigationPlacement Navigation => NavigationPlacement.Root;
    public IList<IResourceListColumn> Columns() => [];
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
    public int Order => 0;
    public string Name => "Pods";
    public ResourceOrigin Origin => ResourceOrigin.BuiltIn;
    public Style[] ListStyle() => [];
    public Type Type { get; } = typeof(V1Pod);
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [(Verb.List, null), (Verb.Watch, null), (Verb.Create, "portforward")];

    public async Task EvaluateListWatchAccessAsync()
    {
        await _releaseTask.ConfigureAwait(false);
        CanListAndWatch = true;
        PermissionsLoaded = true;
    }

    public Task SeedResource(bool waitForReady = false) => Task.CompletedTask;

    public void Initialize(ClusterWorkspace cluster) => Cluster = cluster;
}

internal sealed class ImmediatePermissionResourceConfig : IResourceConfig
{
    private readonly TaskCompletionSource<object?> _completion;

    public ImmediatePermissionResourceConfig(Type type, string name, TaskCompletionSource<object?> completion)
    {
        Type = type;
        Name = name;
        _completion = completion;
    }

    public ClusterWorkspace? Cluster { get; private set; }
    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; private set; }
    public bool PermissionsLoaded { get; private set; }
    public bool ShowNewResource => true;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public NavigationPlacement Navigation => NavigationPlacement.Root;
    public IList<IResourceListColumn> Columns() => [];
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
    public int Order => 0;
    public string Name { get; }
    public ResourceOrigin Origin => ResourceOrigin.BuiltIn;
    public Style[] ListStyle() => [];
    public Type Type { get; }
    public IRelayCommand NewResourceCommand => throw new NotImplementedException();
    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [(Verb.List, null), (Verb.Watch, null)];

    public Task EvaluateListWatchAccessAsync()
    {
        CanListAndWatch = true;
        PermissionsLoaded = true;
        _completion.TrySetResult(null);
        return Task.CompletedTask;
    }

    public Task SeedResource(bool waitForReady = false) => Task.CompletedTask;

    public void Initialize(ClusterWorkspace cluster) => Cluster = cluster;
}
