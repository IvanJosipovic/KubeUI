using k8s;
using k8s.Models;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Clusters.Presentation;

/// <summary>Creates cluster-backed preview data for Avalonia design surfaces.</summary>
public static class DesignTimePreview
{
    /// <summary>Gets whether Avalonia is rendering a design-time preview.</summary>
    public static bool IsDesignMode => Design.IsDesignMode;

    private static IServiceProvider Services
    {
        get
        {
            if (!IsDesignMode)
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

    /// <summary>Gets a service from the current design-time application container.</summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <returns>Resolved service.</returns>
    public static T Get<T>()
        where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>Schedules an action only while rendering a design-time preview.</summary>
    /// <param name="action">Action to run on Avalonia's UI thread.</param>
    public static void Run(Action action)
    {
        if (!IsDesignMode)
        {
            return;
        }

        Dispatcher.UIThread.Post(action);
    }

    /// <summary>Schedules asynchronous work only while rendering a design-time preview.</summary>
    /// <param name="action">Work to run on Avalonia's UI thread.</param>
    public static void Run(Func<Task> action)
    {
        if (!IsDesignMode)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => _ = action());
    }

    /// <summary>Creates and seeds the default cluster with one resource for a design-time preview.</summary>
    /// <typeparam name="TResource">Resource type to seed.</typeparam>
    /// <returns>Connected cluster workspace.</returns>
    public static async Task<ClusterWorkspace> CreateClusterAsync<TResource>()
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var cluster = Services.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
        await cluster.Connect().ConfigureAwait(false);
        await cluster.Runtime.SeedResource<TResource>().ConfigureAwait(false);
        return cluster;
    }

    /// <summary>Creates a cluster-bound view model for a design-time preview.</summary>
    /// <typeparam name="TViewModel">View model type.</typeparam>
    /// <typeparam name="TResource">Resource type to seed.</typeparam>
    /// <returns>Initialized view model.</returns>
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
