using System.Collections;
using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Input;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes.Resources.GatewayApi;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Shell.Navigation;
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

    [AvaloniaFact]
    public async Task gateway_api_resources_get_their_own_category_with_experimental_group()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        var httpRoute = new FakeGatewayApiResourceConfig("HTTPRoute", "HTTP Routes", GatewayApiChannel.Standard);
        var tcpRoute = new FakeGatewayApiResourceConfig("TCPRoute", "TCP Routes", GatewayApiChannel.Experimental);
        IResourceConfig[] configs = [definition, httpRoute, tcpRoute];
        var node = new ClusterNavigationNode(workspace);
        var nodes = new Dictionary<ClusterWorkspace, ClusterNavigationNode> { [workspace] = node };
        var synchronizer = CreateSynchronizer(services);

        synchronizer.Apply(workspace, definition, configs, nodes);
        synchronizer.Apply(workspace, httpRoute, configs, nodes);
        synchronizer.Apply(workspace, tcpRoute, configs, nodes);

        var gatewayApi = node.NavigationItems.Single(item => item.Name == ResourceCategories.GatewayApi);
        gatewayApi.Order.ShouldBe(ResourceCategories.GatewayApiNavigationOrder);
        gatewayApi.NavigationItems.OfType<ResourceNavigationLink>().Single().ResourceKind.ShouldBe(httpRoute.Kind);
        var experimental = gatewayApi.NavigationItems.Single(item => item.Name == Assets.Resources.NavigationView_GatewayApiExperimental);
        experimental.NavigationItems.OfType<ResourceNavigationLink>().Single().ResourceKind.ShouldBe(tcpRoute.Kind);
        var crdRoot = node.NavigationItems.Single(item => item.Name == ResourceCategories.CustomResourceDefinitions);
        crdRoot.NavigationItems.ShouldNotContain(item => item.Name == GatewayApiResources.Group);

        tcpRoute.CanListAndWatch = false;
        synchronizer.Apply(workspace, tcpRoute, configs, nodes);
        gatewayApi.NavigationItems.ShouldNotContain(item => item.Name == Assets.Resources.NavigationView_GatewayApiExperimental);

        httpRoute.PermissionsLoaded = false;
        synchronizer.Apply(workspace, httpRoute, configs, nodes);
        node.NavigationItems.ShouldNotContain(item => item.Name == ResourceCategories.GatewayApi);
    }

    [AvaloniaFact]
    public async Task gateway_api_resources_are_excluded_when_custom_resource_root_is_created()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var definition = new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions");
        var httpRoute = new FakeGatewayApiResourceConfig("HTTPRoute", "HTTP Routes", GatewayApiChannel.Standard);
        var node = new ClusterNavigationNode(workspace);
        var nodes = new Dictionary<ClusterWorkspace, ClusterNavigationNode> { [workspace] = node };

        CreateSynchronizer(services).Apply(workspace, definition, [definition, httpRoute], nodes);

        var crdRoot = node.NavigationItems.Single(item => item.Name == ResourceCategories.CustomResourceDefinitions);
        crdRoot.NavigationItems.OfType<ResourceNavigationLink>().ShouldAllBe(link => link.ResourceKind != httpRoute.Kind);
        crdRoot.NavigationItems.ShouldNotContain(item => item.Name == GatewayApiResources.Group);
    }

    private static NavigationResourceSynchronizer CreateSynchronizer(IServiceProvider services) => new(
        services.GetRequiredService<IResourceIconService>(),
        openCommand: null,
        openInNewTabCommand: null,
        services.GetRequiredService<ILogger<NavigationResourceSynchronizer>>());

    private sealed class FakeGatewayApiResourceConfig(string kind, string name, GatewayApiChannel channel) : IResourceConfig, IGatewayApiResourceConfig
    {
        public GatewayApiChannel? GatewayApiChannel { get; } = channel;
        public ClusterWorkspace? Cluster { get; private set; }
        public bool IsNamespaced => true;
        public bool CanListAndWatch { get; set; } = true;
        public bool PermissionsLoaded { get; set; } = true;
        public bool ShowNewResource => true;
        public bool IsCustomResource => true;
        public GroupApiVersionKind Kind { get; } = new(GatewayApiResources.Group, "v1", kind, kind.ToLowerInvariant() + "s");
        public IList<IResourceListColumn> Columns() => [];
        public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => [];
        public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => [];
        public int Order => 0;
        public string Name { get; } = name;
        public string? Category => ResourceCategories.GatewayApi;
        public global::Avalonia.Styling.Style[] ListStyle() => [];
        public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
        public Task EvaluateListWatchAccessAsync() => Task.CompletedTask;
        public Task SeedResource(bool waitForReady = false) => Task.CompletedTask;
        public Type Type => typeof(GenericKubernetesObject);
        public IRelayCommand NewResourceCommand => throw new NotSupportedException();
        public IRelayCommand<IList> ViewCommand => throw new NotSupportedException();
        public IAsyncRelayCommand<IList> DeleteCommand => throw new NotSupportedException();

        public void Initialize(ClusterWorkspace cluster)
        {
            Cluster = cluster;
        }
    }
}
