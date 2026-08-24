using System.Reactive.Linq;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationViewTests
{
    [Fact]
    public void navigation_item_name_comparer_handles_nulls_and_names()
    {
        var comparer = new NavigationItemNameComparer();
        var first = new NavigationItem { Name = "a" };
        var second = new NavigationItem { Name = "b" };

        comparer.Compare(null, second).ShouldBe(0);
        comparer.Compare(first, null).ShouldBeGreaterThan(0);
        comparer.Compare(first, second).ShouldBeLessThan(0);
    }

    [Fact]
    public void navigation_item_order_comparer_uses_reference_order_name_and_id()
    {
        var comparer = new NavigationItemOrderComparer();
        var first = new NavigationItem { Id = "a", Name = "same", Order = 1 };
        var second = new NavigationItem { Id = "b", Name = "same", Order = 1 };
        var later = new NavigationItem { Id = "c", Name = "later", Order = 2 };

        comparer.Compare(first, first).ShouldBe(0);
        comparer.Compare(null, first).ShouldBeLessThan(0);
        comparer.Compare(first, null).ShouldBeGreaterThan(0);
        comparer.Compare(first, later).ShouldBeLessThan(0);
        comparer.Compare(first, second).ShouldBeLessThan(0);
        comparer.Compare(second, first).ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void cluster_navigation_name_update_with_same_name_preserves_child_ids()
    {
        var workspace = Application.Current.GetTestServices()
            .GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var clusterNode = new ClusterNavigationNode(workspace);
        var child = new NavigationItem
        {
            Id = $"{workspace.Runtime.Name}-child",
            Name = "Child"
        };
        clusterNode.NavigationItems.Add(child);

        clusterNode.UpdateNavigationName(workspace.Runtime.Name);

        child.Id.ShouldBe($"{workspace.Runtime.Name}-child");
    }

    [AvaloniaFact]
    public void navigation_converters_handle_valid_and_invalid_values()
    {
        var culture = CultureInfo.InvariantCulture;
        var geometry = new StringToGeometryConverter();
        var svg = new StringToSvgImageConverter();

        geometry.Convert("M 0,0 L 1,1", typeof(object), null, culture).ShouldNotBeNull();
        geometry.Convert(string.Empty, typeof(object), null, culture).ShouldBeNull();
        geometry.Convert(42, typeof(object), null, culture).ShouldBeNull();
        geometry.ConvertBack(null, typeof(object), null, culture).ShouldBe(BindingOperations.DoNothing);

        svg.Convert("/Assets/kube/blank.svg", typeof(object), null, culture).ShouldNotBeNull();
        svg.Convert(" ", typeof(object), null, culture).ShouldBeNull();
        svg.Convert(42, typeof(object), null, culture).ShouldBeNull();
        svg.ConvertBack(null, typeof(object), null, culture).ShouldBe(BindingOperations.DoNothing);
    }

    [AvaloniaFact]
    public async Task resource_count_assigned_after_template_creation_is_rendered()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<TestClusterConfig>();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = new ClusterNavigationNode(workspace) { IsExpanded = true };
        var podsLink = new ResourceNavigationLink
        {
            Cluster = workspace,
            Id = "test-pods",
            Name = "Pods",
            ResourceKind = GroupApiVersionKind.From<V1Pod>(),
        };
        clusterNode.NavigationItems.Add(podsLink);
        navigation.Clusters.Add(clusterNode);

        var view = new NavigationView { DataContext = navigation };
        var window = new Window { Content = view };
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();
        await TestApplicationExtensions.WaitForUiAsync();

        podsLink.Count = Observable.Return(3);
        await TestApplicationExtensions.WaitForUiAsync();
        await TestApplicationExtensions.WaitForUiAsync();

        view.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).ShouldContain("3");

        window.Content = null;
        window.Close();
    }
}
