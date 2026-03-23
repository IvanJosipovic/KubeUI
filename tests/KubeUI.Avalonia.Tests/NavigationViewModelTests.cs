using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.ViewModels;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;
using CommunityToolkit.Mvvm.Input;

namespace KubeUI.Avalonia.Tests;

public class NavigationViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];

    public override void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    private ClusterWorkspaceViewModel CreateWorkspace(TestCluster runtime)
    {
        var workspace = runtime.CreateWorkspace();
        _disposables.Add(workspace);
        return workspace;
    }

    private NavigationViewModel CreateViewModel()
    {
        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        _disposables.Add(vm);
        return vm;
    }

    private static async Task<T?> WaitForValueAsync<T>(Func<T?> getValue, int timeoutMs = 3000) where T : class
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();

            var value = getValue();
            if (value != null)
            {
                return value;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        return getValue();
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static ResourceNavigationLink? FindResourceLink(ClusterNavigationNode root, string name)
    {
        return FindResourceLink(root.NavigationItems, name);
    }

    private static ResourceNavigationLink? FindResourceLink(IEnumerable<NavigationItem> items, string name)
    {
        foreach (var child in items)
        {
            if (child is ResourceNavigationLink resourceLink && string.Equals(resourceLink.Name, name, StringComparison.Ordinal))
            {
                return resourceLink;
            }

            var nested = FindResourceLink(child.NavigationItems, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    [AvaloniaFact]
    public async Task resource_navigation_items_populate_only_after_connect_completes()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var releaseConnect = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        runtime.ConnectBehavior = async () =>
        {
            runtime.Status = ClusterStatus.Connecting;
            await releaseConnect.Task;
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        clusterNode.NavigationItems.Count.ShouldBe(0);

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.NavigationItems.Count.ShouldBe(0);
        clusterNode.IsExpanded.ShouldBeFalse();

        releaseConnect.TrySetResult(null);

        await WaitForAsync(() => clusterNode.NavigationItems.Count > 0);
        clusterNode.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_does_not_crash_when_connect_fails()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ConnectBehavior = () => throw new InvalidOperationException("connect failed"),
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.NavigationItems.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task cluster_node_expands_after_successful_connect()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.IsExpanded.ShouldBeFalse();

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.Cluster.Connected.ShouldBeTrue();
        clusterNode.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task cluster_node_expands_when_status_becomes_connected_before_permission_refresh_finishes()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var permissionRefreshRelease = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new SlowPermissionResourceConfig(typeof(TestPermissionResourceGamma), "Gamma Permission Resource", permissionRefreshRelease.Task));

        runtime.ConnectBehavior = () =>
        {
            runtime.Status = ClusterStatus.Connected;
            runtime.Connected = true;
            return Task.CompletedTask;
        };

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        vm.TreeView_SelectionChanged(clusterNode);

        await WaitForAsync(() => clusterNode.IsExpanded);
        clusterNode.NavigationItems.Count.ShouldBeGreaterThan(0);

        permissionRefreshRelease.TrySetResult(null);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_does_not_block_on_slow_synchronous_connect_startup()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var releaseConnect = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        runtime.ConnectBehavior = async () =>
        {
            Thread.Sleep(300);
            runtime.Status = ClusterStatus.Connecting;
            await releaseConnect.Task;
            runtime.Connected = true;
            runtime.Status = ClusterStatus.Connected;
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        var stopwatch = Stopwatch.StartNew();
        vm.TreeView_SelectionChanged(clusterNode);
        stopwatch.Stop();

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(150));
        clusterNode.IsExpanded.ShouldBeFalse();

        releaseConnect.TrySetResult(null);

        await WaitForAsync(() => clusterNode.IsExpanded);
        clusterNode.Cluster.Connected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task navigation_items_rebuild_after_resource_config_batch_publish()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        var subtreeChanges = 0;
        originalRoot.NavigationItems.CollectionChanged += (_, _) => subtreeChanges++;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var updatedRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        ReferenceEquals(originalRoot, updatedRoot).ShouldBeTrue();
        subtreeChanges.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public async Task resource_navigation_items_appear_incrementally_as_permissions_complete()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var alphaConfig = new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource");
        var betaConfig = new FakeResourceConfig(typeof(TestPermissionResourceBeta), "Beta Permission Resource");

        workspace.AddResourceConfigForTest(alphaConfig);

        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            workspace.AddResourceConfigForTest(betaConfig);
        });

        var alphaLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, alphaConfig.Name),
            timeoutMs: 150);

        alphaLink.ShouldNotBeNull();
        FindResourceLink(clusterNode, betaConfig.Name).ShouldBeNull();

        var betaLink = await WaitForValueAsync(
            () => FindResourceLink(clusterNode, betaConfig.Name),
            timeoutMs: 1000);

        betaLink.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task permission_driven_resource_add_keeps_existing_navigation_nodes()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var originalNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(TestPermissionResourceAlpha), "Alpha Permission Resource"));
        await WaitForAsync(
            () => FindResourceLink(clusterNode, "Alpha Permission Resource") != null,
            timeoutMs: 1000);

        var updatedNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(originalNamespaceLink, updatedNamespaceLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void port_forwarders_is_under_network_category_not_top_level()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        var networkCategory = clusterNode.NavigationItems.Single(x => x.Name == "Network");
        var portForwardersInNetwork = networkCategory.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        portForwardersInNetwork.ShouldNotBeNull();

        var topLevelPortForwarders = clusterNode.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        topLevelPortForwarders.ShouldBeNull();
    }

    [AvaloniaFact]
    public void port_forwarders_is_hidden_when_pod_portforward_is_not_allowed()
    {
        var runtime = new TestCluster
        {
            CanCreatePodPortForward = false,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var networkCategory = clusterNode.NavigationItems.Single(x => x.Name == "Network");

        var portForwardersInNetwork = networkCategory.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        portForwardersInNetwork.ShouldBeNull();
    }

    [AvaloniaFact]
    public void custom_resource_definitions_link_is_sorted_to_bottom()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.NavigationItems.Last().Name.ShouldBe("Custom Resource Definitions");
    }

    [AvaloniaFact]
    public void category_nav_items_follow_alpha_ordering()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var categoriesByName = clusterNode.NavigationItems
            .ToDictionary(x => x.Name, x => x.Order, StringComparer.Ordinal);

        categoriesByName["Workloads"].ShouldBeLessThan(categoriesByName["Configuration"]);
        categoriesByName["Configuration"].ShouldBeLessThan(categoriesByName["Network"]);
        categoriesByName["Network"].ShouldBeLessThan(categoriesByName["Storage"]);
        categoriesByName["Storage"].ShouldBeLessThan(categoriesByName["Access Control"]);
    }

    [AvaloniaFact]
    public void custom_resource_items_grouped_under_crd_link()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceBeta), "Beta Resources"));
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceNested), "Nested Resources"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = clusterNode.NavigationItems
            .Single(x => x.Name == "Custom Resource Definitions");

        var rootNames = crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .Select(x => x.Name)
            .ToList();

        rootNames.ShouldContain("Definitions");
        rootNames.ShouldContain("kubeui.com");

        var definitionsLink = crdRoot.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1CustomResourceDefinition));

        definitionsLink.Name.ShouldBe("Definitions");

        var rootGroup = crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "kubeui.com");

        var alphaGroup = rootGroup.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "alpha.kubeui.com");

        alphaGroup.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single()
            .ControlType
            .ShouldBe(typeof(TestCustomResourceAlpha));

        var testGroup = rootGroup.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "test.kubeui.com");

        var nestedGroup = testGroup.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "mygroup.test.kubeui.com");

        nestedGroup.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single()
            .ControlType
            .ShouldBe(typeof(TestCustomResourceNested));

        clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Any(x => x.ControlType == typeof(TestCustomResourceAlpha))
            .ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task custom_resource_definitions_root_preserves_expansion_on_rebuild()
    {
        var runtime = new TestCluster();
        var workspace = CreateWorkspace(runtime);
        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        crdRoot.IsExpanded = true;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceBeta), "Beta Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var rebuiltRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        rebuiltRoot.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task resource_navigation_links_keep_counts_after_rebuild()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var resourceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .FirstOrDefault(x => x.ControlType == typeof(V1Namespace));

        resourceLink.ShouldNotBeNull();
        resourceLink.Count.ShouldNotBeNull();

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var rebuiltResourceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .FirstOrDefault(x => x.ControlType == typeof(V1Namespace));

        rebuiltResourceLink.ShouldNotBeNull();
        rebuiltResourceLink.Count.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task crd_delta_does_not_rebuild_unrelated_resource_nodes()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var namespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        var rebuiltNamespaceLink = clusterNode.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1Namespace));

        ReferenceEquals(namespaceLink, rebuiltNamespaceLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_added_after_navigation_build_adds_custom_resource_entry()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);

        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        await runtime.AddOrUpdateResource(new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = "tests.kubeui.com"
            },
            Spec = new()
            {
                Group = "kubeui.com",
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = "tests",
                    Singular = "test",
                    Kind = "Test",
                    ListKind = "TestList"
                },
                Versions =
                [
                    new()
                    {
                        Name = "v1beta1",
                        Served = true,
                        Storage = true,
                        Schema = new()
                        {
                            OpenAPIV3Schema = new()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, V1JSONSchemaProps>
                                {
                                    ["apiVersion"] = new() { Type = "string" },
                                    ["kind"] = new() { Type = "string" },
                                    ["metadata"] = new() { Type = "object" },
                                    ["spec"] = new()
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, V1JSONSchemaProps>
                                        {
                                            ["someString"] = new() { Type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ]
            }
        });

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var testsLink = await WaitForValueAsync(
            () => clusterNode.NavigationItems
                .OfType<NavigationItem>()
                .Where(x => x.Name == "Custom Resource Definitions")
                .SelectMany(x => x.NavigationItems.OfType<NavigationItem>())
                .Where(x => x.Name == "kubeui.com")
                .SelectMany(x => x.NavigationItems.OfType<ResourceNavigationLink>())
                .SingleOrDefault(x => x.Name == "Tests"),
            timeoutMs: 10000);

        testsLink.ShouldNotBeNull();
        testsLink.ControlType
            .ShouldNotBe(typeof(V1CustomResourceDefinition));
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_update_updates_existing_navigation_entry_without_replacing_group()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        await runtime.AddOrUpdateResource(NavigationTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "Tests", "someString"));

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();

        var rootGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        rootGroup.ShouldNotBeNull();
        var originalRootGroup = rootGroup;

        var originalLink = await WaitForValueAsync(
            () => rootGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Tests"),
            timeoutMs: 10000);
        originalLink.ShouldNotBeNull();

        await runtime.AddOrUpdateResource(NavigationTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "Renamed Tests", "otherString"));

        var updatedRootGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        updatedRootGroup.ShouldNotBeNull();
        ReferenceEquals(originalRootGroup, updatedRootGroup).ShouldBeTrue();

        var updatedLink = await WaitForValueAsync(
            () => updatedRootGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Renamed Tests"),
            timeoutMs: 10000);
        updatedLink.ShouldNotBeNull();
        ReferenceEquals(originalLink, updatedLink).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_delete_removes_navigation_entry_without_rebuilding_remaining_groups()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var crdA = NavigationTestCustomResourceDefinitionFactory.Create("tests.kubeui.com", "Tests", "someString");
        var crdB = NavigationTestCustomResourceDefinitionFactory.Create("others.kubeui.com", "Others", "otherString");

        await runtime.AddOrUpdateResource(crdA);
        await runtime.AddOrUpdateResource(crdB);

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();

        var survivingGroup = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        survivingGroup.ShouldNotBeNull();
        var originalSurvivingGroup = survivingGroup;

        await runtime.DeleteResource(crdA);
        await WaitForValueAsync(
            () => survivingGroup.NavigationItems.OfType<ResourceNavigationLink>().SingleOrDefault(x => x.Name == "Others"),
            timeoutMs: 10000);

        crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .SelectMany(x => x.NavigationItems.OfType<ResourceNavigationLink>())
            .Any(x => x.Name == "Tests")
            .ShouldBeFalse();

        var updatedSurvivingGroup = crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "kubeui.com");
        ReferenceEquals(originalSurvivingGroup, updatedSurvivingGroup).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task custom_resource_definition_delete_prunes_empty_group_branch_without_replacing_root()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = CreateWorkspace(runtime);
        var vm = CreateViewModel();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var crd = NavigationTestCustomResourceDefinitionFactory.Create(
            name: "widgets.alpha.kubeui.com",
            plural: "Widgets",
            schemaProperty: "someString",
            group: "alpha.kubeui.com");

        await runtime.AddOrUpdateResource(crd);

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = await WaitForValueAsync(
            () => clusterNode.NavigationItems.SingleOrDefault(x => x.Name == "Custom Resource Definitions"),
            timeoutMs: 10000);
        crdRoot.ShouldNotBeNull();
        var originalRoot = crdRoot;

        var groupBranch = await WaitForValueAsync(
            () => crdRoot.NavigationItems.OfType<NavigationItem>().SingleOrDefault(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);
        groupBranch.ShouldNotBeNull();

        await runtime.DeleteResource(crd);

        await WaitForAsync(
            () => !crdRoot.NavigationItems
                .OfType<NavigationItem>()
                .Any(x => x.Name == "kubeui.com"),
            timeoutMs: 10000);

        var updatedRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        ReferenceEquals(originalRoot, updatedRoot).ShouldBeTrue();
        updatedRoot.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.ControlType == typeof(V1CustomResourceDefinition));
        updatedRoot.NavigationItems
            .Where(x => x is not ResourceNavigationLink)
            .Any()
            .ShouldBeFalse();
    }
}

internal static class NavigationTestCustomResourceDefinitionFactory
{
    public static V1CustomResourceDefinition Create(string name, string plural, string schemaProperty, string group = "kubeui.com")
    {
        return new V1CustomResourceDefinition
        {
            Metadata = new()
            {
                Name = name
            },
            Spec = new()
            {
                Group = group,
                Scope = "Namespaced",
                Names = new()
                {
                    Plural = plural.ToLowerInvariant().Replace(' ', '-'),
                    Singular = "test",
                    Kind = "Test",
                    ListKind = "TestList"
                },
                Versions =
                [
                    new()
                    {
                        Name = "v1beta1",
                        Served = true,
                        Storage = true,
                        Schema = new()
                        {
                            OpenAPIV3Schema = new()
                            {
                                Type = "object",
                                Properties = new Dictionary<string, V1JSONSchemaProps>
                                {
                                    ["apiVersion"] = new() { Type = "string" },
                                    ["kind"] = new() { Type = "string" },
                                    ["metadata"] = new() { Type = "object" },
                                    ["spec"] = new()
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, V1JSONSchemaProps>
                                        {
                                            [schemaProperty] = new() { Type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                ]
            }
        };
    }
}

[KubernetesEntity(Group = "alpha.kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceAlpha")]
internal class TestCustomResourceAlpha : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "alpha.kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceAlpha";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "beta.kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceBeta")]
internal class TestCustomResourceBeta : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "beta.kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceBeta";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "mygroup.test.kubeui.com", ApiVersion = "v1", Kind = "TestCustomResourceNested")]
internal class TestCustomResourceNested : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "mygroup.test.kubeui.com/v1";
    public string Kind { get; set; } = "TestCustomResourceNested";
    public V1ObjectMeta Metadata { get; set; } = new();
}

internal class FakeCustomResourceConfig : IResourceConfig
{
    public FakeCustomResourceConfig(Type resourceType, string name, bool canListAndWatch = true)
    {
        Type = resourceType;
        Name = name;
        CanListAndWatch = canListAndWatch;
    }

    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => true;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public IStyle ListStyle() => null;
    public Task UpdatePermissions() => Task.CompletedTask;
    public Type Type { get; }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
}

internal class FakeResourceConfig : IResourceConfig
{
    public FakeResourceConfig(Type resourceType, string name, bool canListAndWatch = true)
    {
        Type = resourceType;
        Name = name;
        CanListAndWatch = canListAndWatch;
    }

    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public IStyle ListStyle() => null;
    public Task UpdatePermissions() => Task.CompletedTask;
    public Type Type { get; }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
}

[KubernetesEntity(Group = "permissions.alpha.kubeui.com", ApiVersion = "v1", Kind = "TestPermissionResourceAlpha")]
internal class TestPermissionResourceAlpha : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "permissions.alpha.kubeui.com/v1";
    public string Kind { get; set; } = "TestPermissionResourceAlpha";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "permissions.beta.kubeui.com", ApiVersion = "v1", Kind = "TestPermissionResourceBeta")]
internal class TestPermissionResourceBeta : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "permissions.beta.kubeui.com/v1";
    public string Kind { get; set; } = "TestPermissionResourceBeta";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "permissions.gamma.kubeui.com", ApiVersion = "v1", Kind = "TestPermissionResourceGamma")]
internal class TestPermissionResourceGamma : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "permissions.gamma.kubeui.com/v1";
    public string Kind { get; set; } = "TestPermissionResourceGamma";
    public V1ObjectMeta Metadata { get; set; } = new();
}

internal sealed class SlowPermissionResourceConfig : IResourceConfig
{
    private readonly Task _permissionRefreshTask;

    public SlowPermissionResourceConfig(Type resourceType, string name, Task permissionRefreshTask)
    {
        Type = resourceType;
        Name = name;
        _permissionRefreshTask = permissionRefreshTask;
    }

    public bool IsNamespaced => true;
    public bool CanListAndWatch { get; set; }
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From(Type);
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order { get; set; }
    public string Name { get; }
    public string? Category => null;
    public IStyle ListStyle() => null;
    public Type Type { get; }

    public async Task UpdatePermissions()
    {
        await _permissionRefreshTask;
        CanListAndWatch = true;
    }

    public IRelayCommand NewResourceCommand => throw new NotImplementedException();

    public IRelayCommand<IList> ViewCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspaceViewModel cluster) { }
}
