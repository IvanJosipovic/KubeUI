using System.Security.Cryptography;
using System.Text;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

/// <summary>Creates and docks pod-log view models.</summary>
public sealed class PodLogsLauncher(
    IServiceProvider serviceProvider,
    IFactory factory,
    ILogger<PodLogsLauncher> logger) : IPodLogsLauncher
{
    /// <inheritdoc />
    public Task LaunchAsync(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, string resourceKind)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return LaunchAsync(cluster, [resource], resourceKind);
    }

    /// <inheritdoc />
    public async Task LaunchAsync(
        ClusterWorkspace cluster,
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
        string resourceKind)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(resources);
        if (resources.Count == 0)
        {
            throw new ArgumentException("At least one resource is required.", nameof(resources));
        }

        var viewModel = serviceProvider.GetRequiredService<PodLogsViewModel>();
        viewModel.Cluster = cluster.Runtime;
        viewModel.SetScopes(resources, resourceKind);
        var scopeResourceKind = viewModel.ScopeResourceKind;
        viewModel.ContainerName = string.Empty;
        viewModel.Id = BuildDocumentId(cluster.Runtime.Name, viewModel.ScopeItems);

        if (!factory.AddToBottom(viewModel))
        {
            viewModel.Dispose();
            return;
        }

        try
        {
            await viewModel.Connect();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error viewing logs for {ResourceCount} selected {Kind} resources.", resources.Count, scopeResourceKind);
            viewModel.ConnectionError = ex.Message;
        }
    }

    /// <inheritdoc />
    public bool CanAddToActive(ClusterWorkspace cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        var bottomDock = factory.GetDockable<Dock.Model.Controls.IToolDock>("BottomDock");
        return bottomDock?.ActiveDockable is PodLogsViewModel viewModel
            && ReferenceEquals(viewModel.Cluster, cluster.Runtime);
    }

    /// <inheritdoc />
    public Task AddToActiveAsync(
        ClusterWorkspace cluster,
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
        string resourceKind)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(resources);
        var bottomDock = factory.GetDockable<Dock.Model.Controls.IToolDock>("BottomDock");
        if (bottomDock?.ActiveDockable is PodLogsViewModel viewModel
            && ReferenceEquals(viewModel.Cluster, cluster.Runtime))
        {
            factory.SetFocusedDockable(bottomDock, viewModel);
            return viewModel.AddScopesAsync(resources, resourceKind);
        }

        return LaunchAsync(cluster, resources, resourceKind);
    }

    private static string BuildDocumentId(string clusterName, IReadOnlyList<PodLogScopeSelectionItem> scopes)
    {
        if (scopes.Count == 1)
        {
            PodLogScopeSelectionItem scope = scopes[0];
            return $"{nameof(PodLogsViewModel)}-{clusterName}-{scope.ResourceKind}-{scope.Resource.Namespace()}-{scope.Resource.Name()}-all";
        }

        var normalizedScopes = string.Join(
            "\n",
            scopes
                .Select(static scope =>
                    $"{scope.ResourceKind}\t{scope.Resource.Namespace()}\t{scope.Resource.Metadata?.Uid ?? scope.Resource.Name()}")
                .Order(StringComparer.Ordinal));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedScopes));
        return $"{nameof(PodLogsViewModel)}-{clusterName}-multi-{Convert.ToHexString(hash.AsSpan(0, 8))}";
    }

}
