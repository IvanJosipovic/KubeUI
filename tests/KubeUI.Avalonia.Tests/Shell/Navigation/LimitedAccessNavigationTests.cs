using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class LimitedAccessNavigationTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task limited_access_with_listable_namespace_shows_namespaced_resources_in_navigation()
    {
        await using var harness = new KubernetesClusterScenarioHarness();
        await harness.InitializeAsync(TestContext.Current.CancellationToken);

        var runtime = await harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: false);
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, runtime);
        var navigation = services.GetRequiredService<NavigationViewModel>();

        navigation.ClusterCatalog.Clusters.Add(workspace);
        await workspace.Connect();
        Dispatcher.UIThread.RunJobs();

        var clusterNode = navigation.Clusters.Single(x => x.Cluster == workspace);
        await navigation.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return FindResourceLink(clusterNode, "Pods") != null
                && FindResourceLink(clusterNode, "Deployments") != null;
        }, timeoutMs: 30000);

        workspace.GetResourceConfig<k8s.Models.V1Pod>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Pod>().CanListAndWatch.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Deployment>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Deployment>().CanListAndWatch.ShouldBeTrue();

        var podsLink = FindResourceLink(clusterNode, "Pods");
        var deploymentsLink = FindResourceLink(clusterNode, "Deployments");
        podsLink.ShouldNotBeNull();
        deploymentsLink.ShouldNotBeNull();

        await navigation.TreeViewSelectionChangedAsync(podsLink).WaitAsync(TestContext.Current.CancellationToken);
        await navigation.TreeViewSelectionChangedAsync(deploymentsLink).WaitAsync(TestContext.Current.CancellationToken);
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (predicate())
            {
                return;
            }

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            await timer.WaitForNextTickAsync(cancellationToken);
        }

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
}
