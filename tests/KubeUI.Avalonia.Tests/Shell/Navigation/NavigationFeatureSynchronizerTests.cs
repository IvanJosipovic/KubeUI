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
}
