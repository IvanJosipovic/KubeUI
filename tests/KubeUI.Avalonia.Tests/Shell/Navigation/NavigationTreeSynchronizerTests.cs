using Avalonia.Headless.XUnit;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Shell.Navigation;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationTreeSynchronizerTests
{
    [AvaloniaFact]
    public async Task standard_resource_link_moves_when_section_changes()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var config = new FakeResourceConfig(typeof(Corev1Event), "Events")
        {
            Navigation = NavigationPlacement.InSection(NavigationSections.WorkloadsId),
        };
        workspace.AddResourceConfigForTest(config);
        using var node = new ClusterNavigationNode(workspace);
        var synchronizer = new NavigationTreeSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null);
        var builder = new NavigationModelBuilder(new NavigationSectionCatalog([]), NullLogger.Instance);

        synchronizer.Reconcile(node, builder.Build(workspace));
        var link = node.NavigationItems
            .Single(item => item.Name == "Workloads")
            .NavigationItems
            .OfType<NavigationLink>()
            .Single(item => item.ResourceKind == GroupApiVersionKind.From<Corev1Event>());

        config.Navigation = NavigationPlacement.InSection(NavigationSections.NetworkId);
        synchronizer.Reconcile(node, builder.Build(workspace));

        node.NavigationItems.Single(item => item.Name == "Network").NavigationItems
            .ShouldContain(link);
        node.NavigationItems.Single(item => item.Name == "Workloads").NavigationItems
            .ShouldNotContain(link);
    }

    [AvaloniaFact]
    public async Task attach_resource_count_ignores_unseeded_container()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var config = new FakeResourceConfig(typeof(TestCustomResourceKubeUi), "Test Customs");
        workspace.AddResourceConfigForTest(config);
        using var node = new ClusterNavigationNode(workspace);
        var synchronizer = new NavigationTreeSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null);
        var builder = new NavigationModelBuilder(new NavigationSectionCatalog([]), NullLogger.Instance);

        synchronizer.Reconcile(node, builder.Build(workspace));
        var link = node.NavigationItems.OfType<NavigationLink>()
            .Single(item => item.ResourceKind == GroupApiVersionKind.From<TestCustomResourceKubeUi>());

        synchronizer.AttachResourceCount(node, GroupApiVersionKind.From<TestCustomResourceKubeUi>());

        link.Count.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_link_is_removed_when_permission_is_lost()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        workspace.AddResourceConfigForTest(definition);
        using var node = new ClusterNavigationNode(workspace);
        var synchronizer = new NavigationTreeSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null);
        var builder = new NavigationModelBuilder(new NavigationSectionCatalog([]), NullLogger.Instance);

        synchronizer.Reconcile(node, builder.Build(workspace));
        node.NavigationItems.ShouldContain(item => item.Name == "Definitions");

        definition.PermissionsLoaded = false;
        synchronizer.Reconcile(node, builder.Build(workspace));

        node.NavigationItems.ShouldNotContain(item => item.Name == "Definitions");
    }

    [AvaloniaFact]
    public async Task custom_resource_link_returns_to_its_group_when_existing_parent_is_stale()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        var custom = new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Widgets");
        workspace.AddResourceConfigForTest(definition);
        workspace.AddResourceConfigForTest(custom);
        using var node = new ClusterNavigationNode(workspace);
        var synchronizer = new NavigationTreeSynchronizer(
            services.GetRequiredService<IResourceIconService>(),
            openCommand: null,
            openInNewTabCommand: null);
        var builder = new NavigationModelBuilder(new NavigationSectionCatalog([]), NullLogger.Instance);

        synchronizer.Reconcile(node, builder.Build(workspace));
        var root = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-custom-resources");
        var group = root.NavigationItems.Single(item => item.Name == "kubeui.com");
        var link = group.NavigationItems.OfType<NavigationLink>().Single(item => item.ResourceKind is not null);
        group.NavigationItems.Remove(link);
        root.NavigationItems.Add(link);

        synchronizer.Reconcile(node, builder.Build(workspace));

        group.NavigationItems.ShouldContain(link);
        root.NavigationItems.ShouldNotContain(link);
    }
}
