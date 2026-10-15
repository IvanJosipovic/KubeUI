using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationFeatureSynchronizerTests
{
    [Fact]
    public void crossplane_diff_feature_uses_top_level_crossplane_group()
    {
        var definition = CrossplaneNavigationFeature.CreateDefinition();

        definition.Placement.ShouldBe(NavigationFeaturePlacement.Root);
        definition.Path.ShouldBe(["Crossplane"]);
    }

    [AvaloniaFact]
    public async Task root_feature_path_is_created_under_cluster_not_custom_resource_definitions()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        using var node = new ClusterNavigationNode(workspace);
        node.NavigationItems.Add(new NavigationItem
        {
            Id = "custom-resource-definitions",
            Name = ResourceCategories.CustomResourceDefinitions,
            Order = ResourceCategories.CustomResourceDefinitionsNavigationOrder
        });
        var definition = new NavigationFeatureDefinition<NavigationViewModel>(
            "test-root-feature",
            "Diff Detection",
            ["Crossplane"],
            NavigationFeaturePlacement.Root,
            100,
            null,
            static _ => true);

        new NavigationFeatureSynchronizer(new NavigationFeatureCatalog([definition])).Update(workspace, node);

        var crossplane = node.NavigationItems.Single(item => item.Name == "Crossplane");
        crossplane.NavigationItems.Single(item => item.Name == "Diff Detection").ShouldBeOfType<NavigationLink>();
        node.NavigationItems.IndexOf(crossplane)
            .ShouldBeGreaterThan(node.NavigationItems.IndexOf(node.NavigationItems.Single(item => item.Name == ResourceCategories.CustomResourceDefinitions)));
        node.NavigationItems
            .SingleOrDefault(item => item.Name == ResourceCategories.CustomResourceDefinitions)
            ?.NavigationItems
            .ShouldNotContain(item => item.Name == "Crossplane");
    }

    [AvaloniaFact]
    public async Task Feature_link_is_refreshed_then_removed_without_pruning_custom_resource_root()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        using var node = new ClusterNavigationNode(workspace);
        var customResourceRoot = new NavigationItem
        {
            Id = "custom-resource-definitions",
            Name = ResourceCategories.CustomResourceDefinitions
        };
        node.NavigationItems.Add(customResourceRoot);
        var isAvailable = true;
        var definition = new NavigationFeatureDefinition<NavigationViewModel>(
            "test-root-feature",
            "Diff Detection",
            ["Crossplane"],
            NavigationFeaturePlacement.Root,
            100,
            null,
            _ => isAvailable);
        var synchronizer = new NavigationFeatureSynchronizer(new NavigationFeatureCatalog([definition]));

        synchronizer.Update(workspace, node);

        var featureGroup = node.NavigationItems.Single(item => item.Name == "Crossplane");
        var link = featureGroup.NavigationItems.Single(item => item.Name == "Diff Detection").ShouldBeOfType<NavigationLink>();
        link.Name = "stale name";
        link.Order = -1;
        link.ViewModelKey = "stale-id";

        synchronizer.Update(workspace, node);

        link.Name.ShouldBe("Diff Detection");
        link.Order.ShouldBe(100);
        link.ViewModelKey.ShouldBe(definition.Id);

        isAvailable = false;
        synchronizer.Update(workspace, node);

        Assert.Same(customResourceRoot, node.NavigationItems.Single(item => item.Id == customResourceRoot.Id));
        node.NavigationItems.ShouldNotContain(item => item.Name == "Crossplane");
    }
}
