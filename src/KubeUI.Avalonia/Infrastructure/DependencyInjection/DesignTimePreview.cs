using Avalonia;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

internal static class DesignTimePreview
{
    private static IServiceProvider Services
    {
        get
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException($"{nameof(DesignTimePreview)} can only be used in design mode.");
            }

            if (Application.Current is not IServiceProviderHost host)
            {
                throw new InvalidOperationException("Application services are not available.");
            }

            return host.Services;
        }
    }

    public static T Get<T>()
        where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    public static void Run(Action action)
    {
        if (!Design.IsDesignMode)
        {
            return;
        }

        Dispatcher.UIThread.Post(action);
    }

    public static void Run(Func<Task> action)
    {
        if (!Design.IsDesignMode)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => _ = action());
    }

    public static async Task<ClusterWorkspaceViewModel> CreateClusterAsync<TResource>()
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var cluster = Services.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
        await cluster.Connect().ConfigureAwait(false);
        await cluster.SeedResource<TResource>().ConfigureAwait(false);
        return cluster;
    }

    public static async Task<TViewModel> CreateClusterBoundViewModelAsync<TViewModel, TResource>()
        where TViewModel : class, IInitializeCluster
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var cluster = await CreateClusterAsync<TResource>().ConfigureAwait(false);
        var viewModel = Services.GetRequiredService<TViewModel>();
        viewModel.Initialize(cluster);
        return viewModel;
    }
}
