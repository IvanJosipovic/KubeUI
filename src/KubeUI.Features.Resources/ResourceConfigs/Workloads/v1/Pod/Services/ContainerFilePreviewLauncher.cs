using Dock.Model.Core;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Services;

/// <summary>Creates and docks container file previews.</summary>
public sealed class ContainerFilePreviewLauncher(
    Func<ContainerFilePreviewViewModel> viewModelFactory,
    IFactory factory) : IContainerFilePreviewLauncher
{
    /// <inheritdoc />
    public async Task ShowAsync(
        ClusterWorkspace cluster,
        V1Pod pod,
        string containerName,
        ContainerFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(pod);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentNullException.ThrowIfNull(entry);

        var id = BuildDocumentId(cluster, pod, containerName, entry.Path);

        if (factory.FindDockableById(id) is ContainerFilePreviewViewModel existing)
        {
            // Reopening the same file focuses its preview and re-reads it, so changed content shows.
            factory.AddToBottom(existing);
            await existing.LoadAsync().ConfigureAwait(true);
            return;
        }

        var viewModel = viewModelFactory();
        viewModel.Cluster = cluster;
        viewModel.Object = pod;
        viewModel.ContainerName = containerName;
        viewModel.Path = entry.Path;
        viewModel.Title = entry.Name;
        viewModel.Id = id;

        if (!factory.AddToBottom(viewModel))
        {
            viewModel.Dispose();
            return;
        }

        await viewModel.LoadAsync().ConfigureAwait(true);
    }

    private static string BuildDocumentId(
        ClusterWorkspace cluster,
        V1Pod pod,
        string containerName,
        string path)
        => $"{nameof(ContainerFilePreviewViewModel)}-{cluster.Runtime.Name}-{pod.Namespace()}-{pod.Name()}-{containerName}-{path}";
}
