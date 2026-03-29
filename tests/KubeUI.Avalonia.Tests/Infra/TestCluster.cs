using Avalonia;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class TestCluster : TestClusterRuntime
{
    private ClusterWorkspaceViewModel? _workspace;

    public static async Task<ClusterWorkspaceViewModel> GetAsync()
    {
        var runtime = new TestCluster();
        await runtime.AddOrUpdateResource(new k8s.Models.V1Namespace
        {
            Metadata = new() { Name = "default" }
        });

        var workspace = runtime.CreateWorkspace();
        workspace.SelectedNamespaces.Add(runtime.Namespaces.Single());
        return workspace;
    }

    public ClusterWorkspaceViewModel CreateWorkspace()
    {
        _workspace ??= ActivatorUtilities.CreateInstance<ClusterWorkspaceViewModel>(
            Application.Current?.GetRequiredService<IServiceProvider>() ?? throw new InvalidOperationException("Avalonia Application.Current is not initialized."),
            this);

        return _workspace;
    }
}

