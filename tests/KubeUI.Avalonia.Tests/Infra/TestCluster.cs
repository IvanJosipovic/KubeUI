using Avalonia;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class TestCluster : TestClusterRuntime
{
    private ClusterWorkspace? _workspace;

    public static async Task<ClusterWorkspace> GetAsync()
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

    public ClusterWorkspace CreateWorkspace()
    {
        _workspace ??= ActivatorUtilities.CreateInstance<ClusterWorkspace>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."),
            this);

        return _workspace;
    }
}

