using System.Collections.ObjectModel;
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
    public async Task LaunchAsync(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, string resourceKind)
    {
        var viewModel = serviceProvider.GetRequiredService<PodLogsViewModel>();
        viewModel.Cluster = cluster.Runtime;
        viewModel.SetScope(resource, resourceKind);
        var scopeResourceKind = viewModel.ScopeResourceKind;
        viewModel.ContainerName = string.Empty;
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>([
            new PodLogContainerSelectionItem(string.Empty, Assets.Resources.PodLogsView_AllContainers, false, true),
        ]);
        viewModel.Id = $"{nameof(PodLogsViewModel)}-{cluster.Runtime.Name}-{scopeResourceKind}-{resource.Namespace()}-{resource.Name()}-all";

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
            logger.LogError(ex, "Error viewing logs for {Kind} {Namespace}/{Name}", scopeResourceKind, resource.Namespace(), resource.Name());
            viewModel.ConnectionError = ex.Message;
        }
    }

}
