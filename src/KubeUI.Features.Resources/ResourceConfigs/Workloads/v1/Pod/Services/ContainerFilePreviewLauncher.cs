using System.Security.Cryptography;
using System.Text;
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
    {
        // Kubernetes names and Linux paths cannot contain NUL, so joining with it keeps component
        // boundaries unambiguous; the digest keeps the id short while staying stable per target.
        var identity = string.Join("\0", cluster.Runtime.Name, pod.Namespace(), pod.Name(), containerName, path);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        return $"{nameof(ContainerFilePreviewViewModel)}-{cluster.Runtime.Name}-{Convert.ToHexString(digest.AsSpan(0, 8))}";
    }
}
