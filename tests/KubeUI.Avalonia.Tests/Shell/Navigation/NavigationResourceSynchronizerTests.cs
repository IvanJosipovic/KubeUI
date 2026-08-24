using Avalonia.Headless.XUnit;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public class NavigationResourceSynchronizerTests
{
    [AvaloniaFact]
    public async Task standard_resource_link_moves_when_category_changes()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var node = new ClusterNavigationNode(workspace);
        var config = new FakeResourceConfig(typeof(Corev1Event), "Events") { Category = "Workloads" };
        var nodes = new Dictionary<ClusterWorkspace, ClusterNavigationNode> { [workspace] = node };
        var synchronizer = new NavigationResourceSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null,
            services.GetRequiredService<ILogger<NavigationResourceSynchronizer>>());

        synchronizer.Apply(workspace, config, [config], nodes);
        var link = node.NavigationItems
            .Single(item => item.Name == "Workloads")
            .NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single();

        config.Category = "Network";
        synchronizer.Apply(workspace, config, [config], nodes);

        node.NavigationItems.Single(item => item.Name == "Network").NavigationItems
            .Single().ShouldBeSameAs(link);
        node.NavigationItems.ShouldNotContain(item => item.Name == "Workloads");
    }

    [AvaloniaFact]
    public async Task count_attachment_ignores_cluster_without_navigation_node()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var synchronizer = new NavigationResourceSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null,
            services.GetRequiredService<ILogger<NavigationResourceSynchronizer>>());

        synchronizer.AttachResourceCount(
            workspace,
            GroupApiVersionKind.From<Corev1Event>(),
            new Dictionary<ClusterWorkspace, ClusterNavigationNode>());
    }

    [AvaloniaFact]
    public async Task custom_resource_root_is_removed_when_definition_permission_is_lost()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        workspace.AddResourceConfigForTest(definition);
        var node = new ClusterNavigationNode(workspace);
        var nodes = new Dictionary<ClusterWorkspace, ClusterNavigationNode> { [workspace] = node };
        var synchronizer = new NavigationResourceSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null,
            services.GetRequiredService<ILogger<NavigationResourceSynchronizer>>());

        synchronizer.Apply(workspace, definition, [definition], nodes);
        node.NavigationItems.ShouldContain(item => item.Name == ResourceCategories.CustomResourceDefinitions);

        definition.PermissionsLoaded = false;
        synchronizer.Apply(workspace, definition, [definition], nodes);

        node.NavigationItems.ShouldNotContain(item => item.Name == ResourceCategories.CustomResourceDefinitions);
    }

    [AvaloniaFact]
    public async Task custom_resource_link_returns_to_its_group_when_existing_parent_is_stale()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        var custom = new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Widgets");
        workspace.AddResourceConfigForTest(definition);
        workspace.AddResourceConfigForTest(custom);
        var node = new ClusterNavigationNode(workspace);
        var nodes = new Dictionary<ClusterWorkspace, ClusterNavigationNode> { [workspace] = node };
        var synchronizer = new NavigationResourceSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null,
            services.GetRequiredService<ILogger<NavigationResourceSynchronizer>>());

        synchronizer.Apply(workspace, definition, [definition, custom], nodes);
        synchronizer.Apply(workspace, custom, [definition, custom], nodes);
        var root = node.NavigationItems.Single(item => item.Name == ResourceCategories.CustomResourceDefinitions);
        var group = root.NavigationItems.Single(item => item.Name == "kubeui.com");
        var link = group.NavigationItems.OfType<ResourceNavigationLink>().Single();
        group.NavigationItems.Remove(link);
        root.NavigationItems.Add(link);

        synchronizer.Apply(workspace, custom, [definition, custom], nodes);

        group.NavigationItems.ShouldContain(link);
        root.NavigationItems.ShouldNotContain(link);
    }
}
