using KubeUI.Avalonia.Services.Settings;
using KubeUI.Desktop;
using Microsoft.Extensions.Hosting;
using Avalonia.Headless.XUnit;
using FluentIcons.Common;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Crossplane;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.FeatureFlags;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationSectionTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task settings_control_helm_and_hubble_navigation_after_restart(bool helmEnabled, bool hubbleEnabled)
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        SettingsPersistenceData settings = new();
        settings.Settings.PreviewFeatures.Helm = helmEnabled;
        settings.Settings.PreviewFeatures.Hubble = hubbleEnabled;
        var persistence = new Mock<ISettingsPersistence>();
        persistence.Setup(value => value.Load()).Returns(settings);
        persistence.Setup(value => value.EnsureDirectoryExists()).Returns(true);
        persistence.Setup(value => value.Save(It.IsAny<SettingsPersistenceData>())).Returns(true);

        using var original = CreateHost();
        AssertNavigation(original, helmEnabled, hubbleEnabled);

        settings.Settings.PreviewFeatures.Helm = !helmEnabled;
        settings.Settings.PreviewFeatures.Hubble = !hubbleEnabled;
        AssertNavigation(original, helmEnabled, hubbleEnabled);

        using var restarted = CreateHost();
        AssertNavigation(restarted, !helmEnabled, !hubbleEnabled);
        AssertNavigation(original, helmEnabled, hubbleEnabled);

        IHost CreateHost() => Program.CreateHostBuilder(
            [], includeOptionalServices: false,
            configureServices: services => services.AddSingleton(persistence.Object),
            settingsOverride: settings).Build();

        void AssertNavigation(IHost host, bool expectedHelm, bool expectedHubble)
        {
            var catalog = host.Services.GetRequiredService<NavigationSectionCatalog>();
            foreach (var id in new[] { HelmNavigationSectionProvider.ChartsDocumentId, HelmNavigationSectionProvider.RepositoriesDocumentId, HelmNavigationSectionProvider.ReleasesDocumentId })
            {
                catalog.TryGetDocument(id, out var document).ShouldBeTrue();
                document.IsAvailable(workspace).ShouldBe(expectedHelm);
            }

            foreach (var id in new[] { HubbleNavigationSectionProvider.FlowsDocumentId, HubbleNavigationSectionProvider.ServiceMapDocumentId })
            {
                catalog.TryGetDocument(id, out var document).ShouldBeTrue();
                document.IsAvailable(workspace).ShouldBe(expectedHubble);
            }

            using var node = new ClusterNavigationNode(workspace);
            CreateBuilder(catalog).Reconcile(node);
            node.NavigationItems.Any(item => item.Id == $"{workspace.Runtime.Name}-section-helm").ShouldBe(expectedHelm);
            var cilium = node.NavigationItems.SingleOrDefault(item => item.Id == $"{workspace.Runtime.Name}-section-cilium");
            (cilium?.NavigationItems.Any(item => item.Name == Assets.Resources.Hubble_GroupName) ?? false).ShouldBe(expectedHubble);
        }
    }

    [Fact]
    public void helm_section_contains_chart_repository_and_release_documents()
    {
        var provider = new HelmNavigationSectionProvider(new TestFeatureClient(helm: true));

        provider.Section.Id.ShouldBe("helm");
        provider.Section.Name.ShouldBe("Helm");
        provider.Section.Icon.ShouldBeNull();
        provider.Section.SvgIcon.ShouldBe(HelmNavigationSectionProvider.IconAssetPath);
        provider.Section.SvgIconDark.ShouldBe(HelmNavigationSectionProvider.IconDarkAssetPath);
        provider.Documents.Select(document => document.Id).ShouldBe(
            [HelmNavigationSectionProvider.ChartsDocumentId, HelmNavigationSectionProvider.RepositoriesDocumentId, HelmNavigationSectionProvider.ReleasesDocumentId]);
        provider.Documents.Select(document => document.Name).ShouldBe(["Charts", "Repositories", "Releases"]);
        provider.Place(CreateResourceConfig("example.com")).ShouldBeNull();

        var catalog = new NavigationSectionCatalog([provider]);
        catalog.TryGetDocument(HelmNavigationSectionProvider.RepositoriesDocumentId, out _).ShouldBeTrue();
        catalog.TryGetDocument(HelmNavigationSectionProvider.ReleasesDocumentId, out _).ShouldBeTrue();
        catalog.TryGetDocument(HelmNavigationSectionProvider.ChartsDocumentId, out _).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task helm_section_shows_all_documents_while_connected_and_prunes_when_disconnected()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([new HelmNavigationSectionProvider(new TestFeatureClient(helm: true))]));

        builder.Reconcile(node);

        var helm = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-helm");
        helm.NavigationItems.OfType<NavigationLink>().Select(link => link.DocumentId).OrderBy(id => id)
            .ShouldBe(new[] { HelmNavigationSectionProvider.ReleasesDocumentId, HelmNavigationSectionProvider.ChartsDocumentId, HelmNavigationSectionProvider.RepositoriesDocumentId }.OrderBy(id => id));

        await workspace.Runtime.Disconnect();
        builder.Reconcile(node);

        node.NavigationItems.ShouldNotContain(item => item.Id == $"{workspace.Runtime.Name}-section-helm");
    }

    [AvaloniaFact]
    public async Task hubble_documents_remain_visible_while_cilium_detection_is_pending()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();

        using var node = new ClusterNavigationNode(workspace);
        var provider = new HubbleNavigationSectionProvider(new TestFeatureClient(hubble: true));
        var builder = CreateBuilder(new NavigationSectionCatalog([provider]));

        builder.Reconcile(node);

        var cilium = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-cilium");
        cilium.Name.ShouldBe(Assets.Resources.NavigationView_Cilium);
        var hubble = cilium.NavigationItems.Single(item => item.Name == Assets.Resources.Hubble_GroupName);
        hubble.NavigationItems.OfType<NavigationLink>().Select(link => link.DocumentId).OrderBy(id => id)
            .ShouldBe(new[] { HubbleNavigationSectionProvider.FlowsDocumentId, HubbleNavigationSectionProvider.ServiceMapDocumentId }.OrderBy(id => id));
    }

    [AvaloniaFact]
    public async Task helm_section_is_hidden_when_feature_flag_is_disabled()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog(
            [new HelmNavigationSectionProvider(new TestFeatureClient(helm: false))]));

        builder.Reconcile(node);

        node.NavigationItems.ShouldNotContain(item => item.Id == $"{workspace.Runtime.Name}-section-helm");
    }

    [AvaloniaFact]
    public async Task helm_feature_flag_is_captured_when_the_section_provider_is_created()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        var featureClient = new TestFeatureClient(helm: true);
        var provider = new HelmNavigationSectionProvider(featureClient);
        featureClient.Set(FeatureFlagKeys.Helm, false);
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([provider]));

        builder.Reconcile(node);

        node.NavigationItems.ShouldContain(item => item.Id == $"{workspace.Runtime.Name}-section-helm");
    }

    [AvaloniaFact]
    public async Task hubble_documents_are_nested_under_cilium_and_hubble()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();

        using var node = new ClusterNavigationNode(workspace);
        var provider = new HubbleNavigationSectionProvider(new TestFeatureClient(hubble: true));
        var builder = CreateBuilder(new NavigationSectionCatalog([provider]));

        builder.Reconcile(node);

        var cilium = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-cilium");
        cilium.Name.ShouldBe(Assets.Resources.NavigationView_Cilium);
        cilium.SvgIcon.ShouldBe(HubbleNavigationSectionProvider.CiliumIconAssetPath);
        cilium.SvgIconDark.ShouldBe(HubbleNavigationSectionProvider.CiliumIconDarkAssetPath);
        var hubble = cilium.NavigationItems.Single(item => item.Name == Assets.Resources.Hubble_GroupName);
        hubble.SvgIcon.ShouldBe(HubbleNavigationSectionProvider.HubbleIconAssetPath);
        hubble.SvgIconDark.ShouldBe(HubbleNavigationSectionProvider.HubbleIconDarkAssetPath);
        hubble.NavigationItems.OfType<NavigationLink>().Select(link => link.DocumentId).OrderBy(id => id)
            .ShouldBe(new[] { HubbleNavigationSectionProvider.FlowsDocumentId, HubbleNavigationSectionProvider.ServiceMapDocumentId }.OrderBy(id => id));
    }

    [AvaloniaFact]
    public async Task helm_section_node_uses_svg_icon_without_fluent_icon()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog(
            [new HelmNavigationSectionProvider(new TestFeatureClient(helm: true))]));

        builder.Reconcile(node);

        var helm = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-helm");
        helm.SvgIcon.ShouldBe(HelmNavigationSectionProvider.IconAssetPath);
        helm.SvgIconDark.ShouldBe(HelmNavigationSectionProvider.IconDarkAssetPath);
        helm.FluentIcon.ShouldBeNull();
    }

    [Fact]
    public void section_svg_icon_overrides_fluent_icon_and_clears_when_removed()
    {
        var items = new System.Collections.ObjectModel.ObservableCollection<NavigationItem>();

        var withSvg = NavigationTreeEditor.EnsureNode(
            items,
            "section",
            "Section",
            1,
            Icon.DataUsage,
            "/Assets/helm/helm-icon-color.svg",
            "/Assets/helm/helm-icon-white.svg");
        withSvg.FluentIcon.ShouldBeNull();
        withSvg.SvgIcon.ShouldBe("/Assets/helm/helm-icon-color.svg");
        withSvg.SvgIconDark.ShouldBe("/Assets/helm/helm-icon-white.svg");

        var fluentOnly = NavigationTreeEditor.EnsureNode(items, "section", "Section", 1, Icon.DataUsage);
        fluentOnly.ShouldBeSameAs(withSvg);
        fluentOnly.FluentIcon.ShouldBe(Icon.DataUsage);
        fluentOnly.SvgIcon.ShouldBeNull();
        fluentOnly.SvgIconDark.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task hubble_documents_are_hidden_when_feature_flag_is_disabled()
    {
        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Resources =
            [
                new V1CustomResourceDefinition
                {
                    Metadata = new V1ObjectMeta { Name = "ciliumnetworkpolicies.cilium.io" }
                }
            ],
            connect: false);
        await workspace.Connect();

        using var node = new ClusterNavigationNode(workspace);
        var provider = new HubbleNavigationSectionProvider(new TestFeatureClient(hubble: false));
        var builder = CreateBuilder(new NavigationSectionCatalog([provider]));
        builder.Reconcile(node);
        builder.Reconcile(node);
        node.NavigationItems.ShouldNotContain(item => item.Id == $"{workspace.Runtime.Name}-section-cilium");
        node.NavigationItems.ShouldNotContain(item => item.Id == $"{workspace.Runtime.Name}-section-cilium");
    }

    [Fact]
    public void navigation_group_carries_theme_specific_svg_icons()
    {
        System.Collections.ObjectModel.ObservableCollection<NavigationItem> items = [];
        var group = new NavigationGroup("hubble", "Hubble", 0, null, "/Assets/cilium/hubble-light.svg", "/Assets/cilium/hubble-dark.svg");

        var children = NavigationTreeEditor.EnsurePath(items, "section-cilium", [group]);
        var hubble = items.Single();

        hubble.SvgIcon.ShouldBe("/Assets/cilium/hubble-light.svg");
        hubble.SvgIconDark.ShouldBe("/Assets/cilium/hubble-dark.svg");
        hubble.FluentIcon.ShouldBeNull();
        children.ShouldBeSameAs(hubble.NavigationItems);
    }

    [Theory]
    [InlineData("crossplane.io", true)]
    [InlineData("pkg.crossplane.io", true)]
    [InlineData("provider.aws.crossplane.io", true)]
    [InlineData("crossplane.io.example", false)]
    [InlineData("notcrossplane.io", false)]
    [InlineData("Crossplane.io", false)]
    public void crossplane_section_matches_exact_group_and_dot_boundary_subgroups(string group, bool expected)
    {
        var provider = new CrossplaneNavigationSectionProvider();

        var placement = provider.Place(CreateResourceConfig(group));

        (placement is not null).ShouldBe(expected);
    }

    [Fact]
    public void crossplane_section_preserves_custom_resource_group_hierarchy()
    {
        var provider = new CrossplaneNavigationSectionProvider();

        var placement = provider.Place(CreateResourceConfig("provider.aws.crossplane.io"));

        placement.ShouldNotBeNull().Groups.Select(group => group.Name).ShouldBe(
            ["crossplane.io", "aws.crossplane.io", "provider.aws.crossplane.io"]);
    }

    [Fact]
    public void crossplane_section_uses_theme_specific_svg_icons()
    {
        var provider = new CrossplaneNavigationSectionProvider();

        provider.Section.Icon.ShouldBeNull();
        provider.Section.SvgIcon.ShouldBe(CrossplaneNavigationSectionProvider.IconAssetPath);
        provider.Section.SvgIconDark.ShouldBe(CrossplaneNavigationSectionProvider.IconDarkAssetPath);
    }

    [Fact]
    public void section_catalog_sorts_sections_and_resolves_documents()
    {
        var later = new TestNavigationSectionProvider("later", "Same Name", 20);
        var alphabeticallyLater = new TestNavigationSectionProvider("alphabetically-later", "Zebra", 10);
        var first = new TestNavigationSectionProvider("first", "Alpha", 10);
        first.SetDocument("Tool", isAvailable: true);

        var catalog = new NavigationSectionCatalog([later, alphabeticallyLater, first]);

        catalog.Providers.Select(provider => provider.Section.Id)
            .ShouldBe(["first", "alphabetically-later", "later"]);
        catalog.TryGetDocument("test-tool", out var document).ShouldBeTrue();
        document.Name.ShouldBe("Tool");
        catalog.TryGetDocument("missing", out _).ShouldBeFalse();
    }

    [Fact]
    public void section_catalog_rejects_duplicate_section_ids()
    {
        Should.Throw<InvalidOperationException>(() => new NavigationSectionCatalog(
        [
            new TestNavigationSectionProvider("duplicate", "First", 1),
            new TestNavigationSectionProvider("duplicate", "Second", 2),
        ]));
    }

    [Fact]
    public void section_catalog_rejects_duplicate_document_ids()
    {
        var first = new TestNavigationSectionProvider("first", "First", 1);
        first.SetDocument("Tool", isAvailable: true);
        var second = new TestNavigationSectionProvider("second", "Second", 2);
        second.SetDocument("Tool", isAvailable: true);

        Should.Throw<InvalidOperationException>(() => new NavigationSectionCatalog([first, second]));
    }

    [Fact]
    public void custom_resource_group_path_handles_core_shallow_and_nested_groups()
    {
        NavigationTreeEditor.ConstructCustomResourceGroupPath(null).ShouldBe(["core"]);
        NavigationTreeEditor.ConstructCustomResourceGroupPath("example.com").ShouldBe(["example.com"]);
        NavigationTreeEditor.ConstructCustomResourceGroupPath("api.provider.example.com")
            .ShouldBe(["example.com", "provider.example.com", "api.provider.example.com"]);
    }

    [AvaloniaFact]
    public async Task configured_custom_resource_navigation_places_link_at_configured_section_and_under_custom_resources()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var resourceConfig = new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources")
        {
            Navigation = NavigationPlacement.InSection(
                NavigationSections.NetworkId,
                [new NavigationGroup("gateway", "Gateway", 0, null)]),
        };
        workspace.AddResourceConfigForTest(resourceConfig);
            var provider = new CrossplaneNavigationSectionProvider();
        using var node = new ClusterNavigationNode(workspace);
        CreateBuilder(new NavigationSectionCatalog([])).Reconcile(node);

        var network = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-network");
        var gateway = network.NavigationItems.Single(item => item.Name == "Gateway");
        var canonical = gateway.NavigationItems.OfType<NavigationLink>()
            .Single(link => link.ResourceKind == resourceConfig.Kind);
        canonical.Id.ShouldBe($"{workspace.Runtime.Name}-resource-{resourceConfig.Kind.Group}/{resourceConfig.Kind.Kind}");

        var customResources = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-custom-resources");
        var group = customResources.NavigationItems.Single(item => item.Name == "kubeui.com");
        var nested = group.NavigationItems.Single(item => item.Name == "alpha.kubeui.com");
        var crdLink = nested.NavigationItems.OfType<NavigationLink>()
            .Single(link => link.ResourceKind == resourceConfig.Kind);
        crdLink.Id.ShouldContain("-section-resource-custom-resources-");
        ReferenceEquals(canonical, crdLink).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task reconcile_preserves_links_moves_placements_updates_documents_and_prunes_empty_groups()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var section = new TestNavigationSectionProvider();
        var overlappingSection = new TestNavigationSectionProvider("overlap-section", "Overlap", 101);
        var catalog = new NavigationSectionCatalog([section, overlappingSection]);
        var resourceConfig = new FakeCustomResourceConfig(typeof(TestCrossplaneManagedResourceDefinition), "Managed Resources");
        workspace.AddResourceConfigForTest(resourceConfig);

        using var node = new ClusterNavigationNode(workspace);
        var unrelated = new NavigationItem { Id = "unrelated", Name = "Unrelated" };
        node.NavigationItems.Add(unrelated);
        var builder = CreateBuilder(catalog);

        void Reconcile() => builder.Reconcile(node);

        Reconcile();

        var root = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-test-section");
        var alias = root.NavigationItems.OfType<NavigationLink>().Single(link => link.ResourceKind == resourceConfig.Kind);
        alias.Id.ShouldContain("-section-resource-test-section-");
        alias.Name.ShouldBe("Managed Resources");
        var overlappingRoot = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-overlap-section");
        overlappingRoot.NavigationItems.OfType<NavigationLink>()
            .Single(link => link.ResourceKind == resourceConfig.Kind).ResourceKind.ShouldBe(resourceConfig.Kind);

        section.Placement = NavigationPlacement.InSection(section.Section.Id,
        [
            new NavigationGroup("group-a", "Group A", 0, null),
        ]);
        Reconcile();
        var groupA = root.NavigationItems.Single(item => item.Name == "Group A");
        groupA.NavigationItems.ShouldContain(item => ReferenceEquals(item, alias));

        section.Placement = NavigationPlacement.InSection(section.Section.Id,
        [
            new NavigationGroup("group-b", "Group B", 0, null),
        ]);
        Reconcile();
        var groupB = root.NavigationItems.Single(item => item.Name == "Group B");
        groupB.NavigationItems.ShouldContain(item => ReferenceEquals(item, alias));
        root.NavigationItems.ShouldNotContain(item => item.Name == "Group A");

        section.SetDocument("First Tool", isAvailable: true);
        Reconcile();
        var document = root.NavigationItems.OfType<NavigationLink>().Single(link => link.DocumentId is not null);
        document.Name.ShouldBe("First Tool");
        var temporaryParent = new NavigationItem { Id = "temporary", Name = "Temporary" };
        node.NavigationItems.Add(temporaryParent);
        root.NavigationItems.Remove(document);
        temporaryParent.NavigationItems.Add(document);
        section.SetDocument("Updated Tool", isAvailable: true);
        Reconcile();

        ReferenceEquals(document, root.NavigationItems.OfType<NavigationLink>().Single(link => link.DocumentId is not null)).ShouldBeTrue();
        document.Name.ShouldBe("Updated Tool");
        temporaryParent.NavigationItems.ShouldBeEmpty();

        section.IsResourceMatched = false;
        section.SetDocument("Updated Tool", isAvailable: false);
        overlappingSection.IsResourceMatched = false;
        Reconcile();

        node.NavigationItems.ShouldContain(item => ReferenceEquals(item, unrelated));
        node.NavigationItems.ShouldContain(item => ReferenceEquals(item, temporaryParent));
        node.NavigationItems.ShouldNotContain(item => item.Id == root.Id);
        node.NavigationItems.ShouldNotContain(item => item.Id == overlappingRoot.Id);
    }

    [AvaloniaFact]
    public async Task alias_identity_survives_and_stale_count_is_cleared_on_resource_version_replacement()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var firstConfig = new FakeCustomResourceConfig(typeof(TestCrossplaneVersionedResourceV1), "Versioned Resources");
        workspace.AddResourceConfigForTest(firstConfig);

        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([new CrossplaneNavigationSectionProvider()]));

        void Reconcile() => builder.Reconcile(node);

        Reconcile();
        var section = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-crossplane");
        var alias = FindResourceLink(section, firstConfig.Kind).ShouldNotBeNull();
        var identity = alias.Id;
        alias.IsExpanded = true;

        var replacementConfig = new FakeCustomResourceConfig(typeof(TestCrossplaneVersionedResourceV2), "Versioned Resources");
        replacementConfig.CanListAndWatch.ShouldBeTrue();
        replacementConfig.PermissionsLoaded.ShouldBeTrue();
        workspace.AddResourceConfigForTest(replacementConfig);
        workspace.GetResourceConfigs().ShouldContain(item => ReferenceEquals(item, replacementConfig));
        workspace.GetResourceConfigs().ShouldNotContain(item => ReferenceEquals(item, firstConfig));
        Reconcile();

        var currentSection = node.NavigationItems.SingleOrDefault(item => item.Id == section.Id).ShouldNotBeNull();
        GetResourceLinks(currentSection).Select(link => link.ResourceKind?.GroupApiVersion)
            .ShouldBe([replacementConfig.Kind.GroupApiVersion]);
        var replacementAlias = FindResourceLink(currentSection, replacementConfig.Kind).ShouldNotBeNull();
        currentSection.ShouldBeSameAs(section);
        replacementAlias.ShouldBeSameAs(alias);
        replacementAlias.Id.ShouldBe(identity);
        replacementAlias.ResourceKind.ShouldBe(replacementConfig.Kind);
        replacementAlias.Count.ShouldBeNull();
        replacementAlias.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task canonical_identity_survives_and_stale_count_is_cleared_on_resource_version_replacement()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var firstConfig = new FakeCustomResourceConfig(typeof(TestCrossplaneVersionedResourceV1), "Versioned Resources");
        workspace.AddResourceConfigForTest(firstConfig);

        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([new CrossplaneNavigationSectionProvider()]));

        void Reconcile() => builder.Reconcile(node);

        Reconcile();
        var customResources = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-custom-resources");
        var canonical = FindResourceLink(customResources, firstConfig.Kind).ShouldNotBeNull();
        var identity = canonical.Id;
        canonical.IsExpanded = true;

        var replacementConfig = new FakeCustomResourceConfig(typeof(TestCrossplaneVersionedResourceV2), "Versioned Resources");
        workspace.AddResourceConfigForTest(replacementConfig);
        Reconcile();

        var replacementCanonical = FindResourceLink(customResources, replacementConfig.Kind).ShouldNotBeNull();
        replacementCanonical.ShouldBeSameAs(canonical);
        replacementCanonical.Id.ShouldBe(identity);
        replacementCanonical.Id.ShouldBe(
            $"{workspace.Runtime.Name}-resource-{replacementConfig.Kind.Group}/{replacementConfig.Kind.Kind}");
        replacementCanonical.Count.ShouldBeNull();
        replacementCanonical.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task aliases_at_different_group_paths_do_not_collide()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        await workspace.Connect();
        workspace.AddResourceConfigForTest(new FakeResourceConfig(typeof(V1CustomResourceDefinition), "Definitions"));
        var resourceConfig = new FakeCustomResourceConfig(typeof(TestCustomResourceAlpha), "Alpha Resources")
        {
            Navigation = NavigationPlacement.InSection(NavigationSections.WorkloadsId),
        };
        workspace.AddResourceConfigForTest(resourceConfig);

        var provider = new TestNavigationSectionProvider(NavigationSections.CustomResources)
        {
            Placement = NavigationPlacement.InSection(
                NavigationSections.CustomResourcesId,
                [new NavigationGroup("provider-group", "Provider", 0, null)]),
        };

        using var node = new ClusterNavigationNode(workspace);
        var builder = CreateBuilder(new NavigationSectionCatalog([provider]));

        builder.Reconcile(node);

        var customResources = node.NavigationItems.Single(item => item.Id == $"{workspace.Runtime.Name}-section-custom-resources");
        var aliases = GetResourceLinks(customResources)
            .Where(link => link.ResourceKind == resourceConfig.Kind)
            .ToArray();
        aliases.Length.ShouldBe(2);
        aliases.Select(link => link.Id).Distinct(StringComparer.Ordinal).Count().ShouldBe(2);
        aliases.ShouldContain(link => link.Id.Contains("-crd-", StringComparison.Ordinal));
        aliases.ShouldContain(link => link.Id.Contains("-provider-", StringComparison.Ordinal));
    }

    private static NavigationTreeBuilder CreateBuilder(NavigationSectionCatalog catalog) => new(
        catalog,
        Application.Current.GetTestServices().GetRequiredService<IResourceIconService>(),
        openCommand: null,
        openInNewTabCommand: null,
        NullLogger.Instance);

    private static NavigationLink? FindResourceLink(NavigationItem root, GroupApiVersionKind kind)
    {
        foreach (var item in root.NavigationItems)
        {
            if (item is NavigationLink resourceLink && resourceLink.ResourceKind == kind)
            {
                return resourceLink;
            }

            var nested = FindResourceLink(item, kind);
            if (nested is not null)
            {
                return nested;
            }

        }

        return null;
    }

    [Theory]
    [InlineData("cilium.io", true)]
    [InlineData("api.cilium.io", true)]
    [InlineData("policy.api.cilium.io", true)]
    [InlineData("cilium.io.example", false)]
    [InlineData("notcilium.io", false)]
    [InlineData("Cilium.io", false)]
    public void cilium_section_matches_exact_group_and_dot_boundary_subgroups(string group, bool expected)
    {
        var provider = new HubbleNavigationSectionProvider(new TestFeatureClient(hubble: true));

        var placement = provider.Place(CreateResourceConfig(group));

        (placement is not null).ShouldBe(expected);
        if (expected)
        {
            placement!.SectionId.ShouldBe(NavigationSections.CiliumId);
        }
    }

    [Fact]
    public void cilium_section_preserves_custom_resource_group_hierarchy()
    {
        var provider = new HubbleNavigationSectionProvider(new TestFeatureClient(hubble: true));

        var placement = provider.Place(CreateResourceConfig("policy.api.cilium.io"));

        placement.ShouldNotBeNull().Groups.Select(group => group.Name).ShouldBe(
            ["cilium.io", "api.cilium.io", "policy.api.cilium.io"]);
    }

    private static IEnumerable<NavigationLink> GetResourceLinks(NavigationItem root)
    {
        foreach (var item in root.NavigationItems)
        {
            if (item is NavigationLink link && link.ResourceKind is not null)
            {
                yield return link;
            }

            foreach (var nested in GetResourceLinks(item))
            {
                yield return nested;
            }
        }
    }

    private static IResourceConfig CreateResourceConfig(string group, string apiVersion = "v1")
    {
        var config = new Mock<IResourceConfig>();
        config.SetupGet(resource => resource.Kind)
            .Returns(new GroupApiVersionKind(group, apiVersion, "TestResource", "testresources"));
        return config.Object;
    }
}

[KubernetesEntity(Group = "versions.crossplane.io", ApiVersion = "v1", Kind = "VersionedResource")]
internal sealed class TestCrossplaneVersionedResourceV1 : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "versions.crossplane.io/v1";
    public string Kind { get; set; } = "VersionedResource";
    public V1ObjectMeta Metadata { get; set; } = new();
}

[KubernetesEntity(Group = "versions.crossplane.io", ApiVersion = "v2", Kind = "VersionedResource")]
internal sealed class TestCrossplaneVersionedResourceV2 : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "versions.crossplane.io/v2";
    public string Kind { get; set; } = "VersionedResource";
    public V1ObjectMeta Metadata { get; set; } = new();
}

internal sealed class TestNavigationSectionProvider : INavigationSectionProvider
{
    private IReadOnlyList<NavigationDocument> _documents = [];

    public TestNavigationSectionProvider(string id = "test-section", string name = "Test Section", int order = 100)
        : this(new NavigationSection(id, name, order, null))
    {
    }

    public TestNavigationSectionProvider(NavigationSection section)
    {
        Section = section;
        Placement = NavigationPlacement.InSection(section.Id);
    }

    public NavigationSection Section { get; }

    public NavigationPlacement Placement { get; set; }

    public bool IsResourceMatched { get; set; } = true;

    public IReadOnlyList<NavigationDocument> Documents => _documents;

    public NavigationPlacement? Place(IResourceConfig resource)
        => IsResourceMatched ? Placement with { SectionId = Section.Id } : null;

    public void SetDocument(string name, bool isAvailable)
    {
        _documents =
        [
            new NavigationDocument(
                "test-tool",
                name,
                20,
                Icon.DataUsage,
                _ => isAvailable,
                static (_, _) => throw new NotSupportedException()),
        ];
    }

    public void SetDocumentCount(int count)
    {
        _documents = Enumerable.Range(0, count)
            .Select(index => new NavigationDocument(
                $"test-tool-{index}",
                $"Tool {index}",
                index,
                null,
                static _ => true,
                static (_, _) => throw new NotSupportedException()))
            .ToArray();
    }
}
