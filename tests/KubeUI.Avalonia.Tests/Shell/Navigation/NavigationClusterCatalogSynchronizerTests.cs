using System.Collections.ObjectModel;
using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Input;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Shell.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationClusterCatalogSynchronizerTests
{
    [AvaloniaFact]
    public async Task subscribing_already_populated_workspace_reconciles_once()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1Pod), "Pods"));
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1Namespace), "Namespaces"));
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1Node), "Nodes"));

        var catalog = Application.Current.GetTestServices().GetRequiredService<ClusterWorkspaceCatalog>();
        catalog.Clusters.Remove(workspace).ShouldBeTrue();

        var nodes = new ObservableCollection<ClusterNavigationNode>();
        var reconciles = 0;
        var command = new RelayCommand(static () => { });
        using var synchronizer = new NavigationClusterCatalogSynchronizer(
            catalog,
            nodes,
            static _ => { },
            static _ => { },
            _ => Interlocked.Increment(ref reconciles),
            command,
            command,
            NullLogger<NavigationClusterCatalogSynchronizer>.Instance);

        try
        {
            catalog.Clusters.Add(workspace);

            nodes.ShouldContain(node => node.Cluster == workspace);
            reconciles.ShouldBe(1);
        }
        finally
        {
            catalog.Clusters.Remove(workspace);
        }
    }
}
