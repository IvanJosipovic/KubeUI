using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [AvaloniaFact]
    public async Task resource_navigation_items_populate_only_after_connect_starts()
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

        clusterNode.NavigationItems.Count.ShouldBe(0);

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.NavigationItems.Count.ShouldBeGreaterThan(0);
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
        var rebuilds = 0;
        clusterNode.NavigationItems.CollectionChanged += (_, _) => rebuilds++;

        workspace.AddResourceConfigForTest(new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources"));
        await Task.Delay(250);
        Dispatcher.UIThread.RunJobs();

        rebuilds.ShouldBeGreaterThan(0);
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

        await Task.Delay(500);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var crdRoot = clusterNode.NavigationItems.Single(x => x.Name == "Custom Resource Definitions");
        var rootGroup = crdRoot.NavigationItems
            .OfType<NavigationItem>()
            .Single(x => x.Name == "kubeui.com");

        rootGroup.NavigationItems
            .OfType<ResourceNavigationLink>()
            .Single(x => x.Name == "Tests")
            .ControlType
            .ShouldNotBe(typeof(V1CustomResourceDefinition));
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

