using System.Diagnostics;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Shell.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationTreeBuilderTests
{
    [AvaloniaFact]
    public async Task standard_resource_link_moves_when_section_changes()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var config = new FakeResourceConfig(typeof(Corev1Event), "Events")
        {
            Navigation = NavigationPlacement.InSection(NavigationSections.WorkloadsId),
        };
        workspace.AddResourceConfigForTest(config);
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([]));

        builder.Reconcile(node);
        var link = node.NavigationItems
            .Single(item => item.Name == "Workloads")
            .NavigationItems
            .OfType<NavigationLink>()
            .Single(item => item.ResourceKind == GroupApiVersionKind.From<Corev1Event>());

        config.Navigation = NavigationPlacement.InSection(NavigationSections.NetworkId);
        builder.Reconcile(node);

        node.NavigationItems.Single(item => item.Name == "Network").NavigationItems
            .ShouldContain(link);
        node.NavigationItems.Single(item => item.Name == "Workloads").NavigationItems
            .ShouldNotContain(link);
    }

    [AvaloniaFact]
    public async Task attach_resource_count_ignores_unseeded_container()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var config = new FakeResourceConfig(typeof(TestCustomResourceKubeUi), "Test Customs");
        workspace.AddResourceConfigForTest(config);
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([]));

        builder.Reconcile(node);
        var link = node.NavigationItems.OfType<NavigationLink>()
            .Single(item => item.ResourceKind == GroupApiVersionKind.From<TestCustomResourceKubeUi>());

        builder.AttachResourceCount(node, GroupApiVersionKind.From<TestCustomResourceKubeUi>());

        link.Count.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_link_is_removed_when_permission_is_lost()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        workspace.AddResourceConfigForTest(definition);
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([]));

        builder.Reconcile(node);
        node.NavigationItems.ShouldContain(item => item.Name == "Definitions");

        definition.PermissionsLoaded = false;
        builder.Reconcile(node);

        node.NavigationItems.ShouldNotContain(item => item.Name == "Definitions");
    }

    [AvaloniaFact]
    public async Task custom_resource_link_returns_to_its_group_when_existing_parent_is_stale()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        var custom = new FakeCustomResourceConfig(typeof(TestCustomResourceKubeUi), "Widgets");
        workspace.AddResourceConfigForTest(definition);
        workspace.AddResourceConfigForTest(custom);
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([]));

        builder.Reconcile(node);
        var root = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-custom-resources");
        var group = root.NavigationItems.Single(item => item.Name == "kubeui.com");
        var link = group.NavigationItems.OfType<NavigationLink>().Single(item => item.ResourceKind is not null);
        group.NavigationItems.Remove(link);
        root.NavigationItems.Add(link);

        builder.Reconcile(node);

        group.NavigationItems.ShouldContain(link);
        root.NavigationItems.ShouldNotContain(link);
    }

    [AvaloniaFact]
    public async Task reconcile_handles_large_existing_navigation_tree_without_repeated_full_tree_scans()
    {
        const int documentCount = 6_000;

        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var provider = new TestNavigationSectionProvider("performance", "Performance", 0);
        provider.SetDocumentCount(documentCount);
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([provider]));

        builder.Reconcile(node);
        var section = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-performance");
        var expectedLinks = section.NavigationItems.OfType<NavigationLink>()
            .Where(link => link.DocumentId is not null)
            .ToArray();
        expectedLinks.Length.ShouldBe(documentCount);

        var stopwatch = Stopwatch.StartNew();
        builder.Reconcile(node);
        stopwatch.Stop();

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(500));
        var documentLinks = section.NavigationItems.OfType<NavigationLink>()
            .Where(link => link.DocumentId is not null)
            .ToArray();
        documentLinks.Length.ShouldBe(documentCount);
        documentLinks.ShouldAllBe(link => expectedLinks.Contains(link, ReferenceEqualityComparer.Instance));
    }

    private static NavigationTreeBuilder CreateBuilder(NavigationSectionCatalog catalog) => new(
        catalog,
        Application.Current.GetTestServices().GetRequiredService<IResourceIconService>(),
        openCommand: null,
        openInNewTabCommand: null,
        NullLogger.Instance);
}
