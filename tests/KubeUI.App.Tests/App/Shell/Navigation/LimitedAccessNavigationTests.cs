using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.App.Shell.Navigation;
using Shouldly;

namespace KubeUI.App.Tests.App.Shell;

public sealed class LimitedAccessNavigationTests
{
    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task limited_access_with_listable_namespace_shows_namespaced_resources_in_navigation(KubernetesBackend backend)
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialYaml = KubernetesTestData.LimitedAccessWithNamespacePermissions;
        });

        var navigation = services.GetRequiredService<NavigationViewModel>();

        var clusterNode = navigation.Clusters.Single(x => x.Cluster == workspace);
        await navigation.TreeViewSelectionChangedAsync(clusterNode);

        await TestWait.UntilAsync(
            () =>
            {
                Dispatcher.UIThread.RunJobs();
                return FindResourceLink(clusterNode, "Pods") != null
                    && FindResourceLink(clusterNode, "Deployments") != null;
            },
            60000,
            TestContext.Current.CancellationToken);

        workspace.GetResourceConfig(GroupApiVersionKind.From<k8s.Models.V1Pod>()).PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig(GroupApiVersionKind.From<k8s.Models.V1Pod>()).CanListAndWatch.ShouldBeTrue();
        workspace.GetResourceConfig(GroupApiVersionKind.From<k8s.Models.V1Deployment>()).PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig(GroupApiVersionKind.From<k8s.Models.V1Deployment>()).CanListAndWatch.ShouldBeTrue();

        var podsLink = FindResourceLink(clusterNode, "Pods");
        var deploymentsLink = FindResourceLink(clusterNode, "Deployments");
        podsLink.ShouldNotBeNull();
        deploymentsLink.ShouldNotBeNull();

        await navigation.TreeViewSelectionChangedAsync(podsLink).WaitAsync(TestContext.Current.CancellationToken);
        await navigation.TreeViewSelectionChangedAsync(deploymentsLink).WaitAsync(TestContext.Current.CancellationToken);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task discovered_custom_resource_is_navigable_without_crd_definition_access(KubernetesBackend backend)
    {
        var definition = TestCustomResourceDefinitionFactory.Create("widgets.kubeui.com", "widgets", "value");
        var kind = new GroupApiVersionKind("kubeui.com", "v1beta1", "Test", "widgets");
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialResources =
            [
                definition,
                new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                new V1ServiceAccount
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = KubernetesRbac.ServiceAccountName,
                        NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                    },
                },
                .. KubernetesRbac.ClusterWide(
                    new RbacRule("namespaces", "list"),
                    new RbacRule("namespaces", "watch")),
                .. KubernetesRbac.InNamespace(
                    "my-app",
                    new RbacRule("widgets", "list", "kubeui.com"),
                    new RbacRule("widgets", "watch", "kubeui.com")),
            ];
        });
        var services = Application.Current.GetTestServices();
        var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = navigation.Clusters.Single(node => node.Cluster == workspace);

        var resourceLink = await TestWait.UntilValueAsync(
            () =>
            {
                Dispatcher.UIThread.RunJobs();
                return FindResourceLink(clusterNode, kind);
            },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        workspace.Runtime.Permissions.CanI<V1CustomResourceDefinition>(Verb.List).ShouldBeFalse();
        workspace.Runtime.Permissions.CanI<V1CustomResourceDefinition>(Verb.Watch).ShouldBeFalse();
        workspace.GetResourceConfig(kind).CanListAndWatch.ShouldBeTrue();
        resourceLink.ShouldNotBeNull();
        await navigation.TreeViewSelectionChangedAsync(resourceLink).WaitAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Finds a matching resource link anywhere beneath a cluster navigation node.</summary>
    private static NavigationLink? FindResourceLink(ClusterNavigationNode root, GroupApiVersionKind kind)
    {
        return FindResourceLink(root.NavigationItems, kind);
    }

    /// <summary>Finds a matching resource link recursively within navigation items.</summary>
    private static NavigationLink? FindResourceLink(IEnumerable<NavigationItem> items, GroupApiVersionKind kind)
    {
        foreach (var child in items)
        {
            if (child is NavigationLink resourceLink && resourceLink.ResourceKind == kind)
            {
                return resourceLink;
            }

            var nested = FindResourceLink(child.NavigationItems, kind);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static NavigationLink? FindResourceLink(ClusterNavigationNode root, string name)
    {
        return FindResourceLink(root.NavigationItems, name);
    }

    private static NavigationLink? FindResourceLink(IEnumerable<NavigationItem> items, string name)
    {
        foreach (var child in items)
        {
            if (child is NavigationLink resourceLink && resourceLink.ResourceKind is not null && string.Equals(resourceLink.Name, name, StringComparison.Ordinal))
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
}
