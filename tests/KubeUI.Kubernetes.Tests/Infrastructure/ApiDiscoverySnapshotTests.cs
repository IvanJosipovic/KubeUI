using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class ApiDiscoverySnapshotTests
{
    [Fact]
    public void Create_ignores_subresources_and_selects_a_deterministic_served_version()
    {
        var core = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items = [],
        };
        var groups = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items =
            [
                new V2beta1APIGroupDiscoveryListItem
                {
                    metadata = new V2beta1APIGroupDiscoveryListItemMetadata { name = "widgets.example.com" },
                    versions =
                    [
                        new V2beta1APIGroupDiscoveryListItemVersion
                        {
                            version = "v1alpha1",
                            freshness = "Current",
                            resources =
                            [
                                CreateResource("widgets", "Widget", "Namespaced", "v1alpha1", "list", "watch"),
                                CreateResource("widgets/status", "Widget", "Namespaced", "v1alpha1", "get"),
                            ],
                        },
                        new V2beta1APIGroupDiscoveryListItemVersion
                        {
                            version = "v1",
                            freshness = "Current",
                            resources = [CreateResource("widgets", "Widget", "Namespaced", "v1", "list", "watch")],
                        },
                        new V2beta1APIGroupDiscoveryListItemVersion
                        {
                            version = "v2beta1",
                            freshness = "Current",
                            resources = [CreateResource("widgets", "Widget", "Namespaced", "v2beta1", "list", "watch")],
                        },
                    ],
                },
            ],
        };

        var snapshot = ApiDiscoverySnapshot.Create(core, groups);

        snapshot.Resources.Count.ShouldBe(3);
        snapshot.Resources.ShouldNotContain(resource => resource.Kind.PluralName == "widgets/status");
        snapshot.PreferredResources.ShouldHaveSingleItem();
        snapshot.PreferredResources[0].Kind.ShouldBe(new GroupApiVersionKind("widgets.example.com", "v1", "Widget", "widgets"));
        snapshot.PreferredResources[0].IsNamespaced.ShouldBeTrue();
        snapshot.PreferredResources[0].Verbs.ShouldContain("list");
    }

    [Fact]
    public void Create_falls_back_to_enclosing_group_and_version_when_both_response_kind_fields_are_empty()
    {
        var core = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items =
            [
                CreateGroup(
                    string.Empty,
                    "v1",
                    new V2beta1APIGroupDiscoveryListItemVersionResource
                    {
                        resource = "pods",
                        responseKind = new V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind
                        {
                            version = "v1",
                            kind = "Pod",
                        },
                        scope = "Namespaced",
                        verbs = ["list", "watch"],
                    }),
            ],
        };
        var groups = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items =
            [
                CreateGroup(
                    "apps",
                    "v1",
                    new V2beta1APIGroupDiscoveryListItemVersionResource
                    {
                        resource = "deployments",
                        responseKind = new V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind
                        {
                            kind = "Deployment",
                        },
                        scope = "Namespaced",
                        verbs = ["list", "watch"],
                    }),
            ],
        };

        var snapshot = ApiDiscoverySnapshot.Create(core, groups);

        snapshot.Resources.ShouldContain(resource => resource.Kind == new GroupApiVersionKind(string.Empty, "v1", "Pod", "pods"));
        snapshot.Resources.ShouldContain(resource => resource.Kind == new GroupApiVersionKind("apps", "v1", "Deployment", "deployments"));
    }

    [Fact]
    public void PreferredResources_orders_versions_numerically_within_a_stability_stage()
    {
        var core = new V2beta1APIGroupDiscoveryList { kind = "APIGroupDiscoveryList", items = [] };
        var groups = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items =
            [
                CreateGroup(
                    "widgets.example.com",
                    "v1beta2",
                    CreateResource("widgets", "Widget", "Namespaced", "v1beta2", "list", "watch")),
                CreateGroup(
                    "widgets.example.com",
                    "v1beta10",
                    CreateResource("widgets", "Widget", "Namespaced", "v1beta10", "list", "watch")),
            ],
        };

        var snapshot = ApiDiscoverySnapshot.Create(core, groups);

        snapshot.PreferredResources.ShouldHaveSingleItem();
        snapshot.PreferredResources[0].Kind.ApiVersion.ShouldBe("v1beta10");
    }

    [Fact]
    public void PreferredResources_prefers_stable_version_over_newer_beta_version()
    {
        var core = new V2beta1APIGroupDiscoveryList { kind = "APIGroupDiscoveryList", items = [] };
        var groups = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items =
            [
                CreateGroup(
                    "widgets.example.com",
                    "v1",
                    CreateResource("widgets", "Widget", "Namespaced", "v1", "list", "watch")),
                CreateGroup(
                    "widgets.example.com",
                    "v2beta1",
                    CreateResource("widgets", "Widget", "Namespaced", "v2beta1", "list", "watch")),
            ],
        };

        var snapshot = ApiDiscoverySnapshot.Create(core, groups);

        snapshot.PreferredResources.ShouldHaveSingleItem();
        snapshot.PreferredResources[0].Kind.ApiVersion.ShouldBe("v1");
    }

    [Fact]
    public void Create_retains_previous_resources_for_stale_versions_until_discovery_is_current()
    {
        var kind = new GroupApiVersionKind("widgets.example.com", "v1", "Widget", "widgets");
        var previous = ApiDiscoverySnapshot.Create(
            new V2beta1APIGroupDiscoveryList { kind = "APIGroupDiscoveryList", items = [] },
            new V2beta1APIGroupDiscoveryList
            {
                kind = "APIGroupDiscoveryList",
                items = [CreateGroup("widgets.example.com", "v1", CreateResource("widgets", "Widget", "Namespaced", "v1", "list", "watch"))],
            });
        var core = new V2beta1APIGroupDiscoveryList { kind = "APIGroupDiscoveryList", items = [] };
        var staleGroups = new V2beta1APIGroupDiscoveryList
        {
            kind = "APIGroupDiscoveryList",
            items =
            [
                new V2beta1APIGroupDiscoveryListItem
                {
                    metadata = new V2beta1APIGroupDiscoveryListItemMetadata { name = "widgets.example.com" },
                    versions =
                    [
                        new V2beta1APIGroupDiscoveryListItemVersion
                        {
                            version = "v1",
                            freshness = "Stale",
                            resources = [],
                        },
                    ],
                },
            ],
        };

        var staleSnapshot = ApiDiscoverySnapshot.Create(core, staleGroups, previous);

        staleSnapshot.Resources.ShouldContain(resource => resource.Kind == kind);
        staleSnapshot.Resources.Single(resource => resource.Kind == kind).Freshness.ShouldBe("Stale");

        staleGroups.items[0].versions[0].freshness = "Current";
        var currentSnapshot = ApiDiscoverySnapshot.Create(core, staleGroups, staleSnapshot);

        currentSnapshot.Resources.ShouldNotContain(resource => resource.Kind == kind);
    }

    private static V2beta1APIGroupDiscoveryListItem CreateGroup(
        string group,
        string version,
        params V2beta1APIGroupDiscoveryListItemVersionResource[] resources)
        => new()
        {
            metadata = new V2beta1APIGroupDiscoveryListItemMetadata { name = group },
            versions =
            [
                new V2beta1APIGroupDiscoveryListItemVersion
                {
                    version = version,
                    freshness = "Current",
                    resources = resources,
                },
            ],
        };

    private static V2beta1APIGroupDiscoveryListItemVersionResource CreateResource(
        string resource,
        string kind,
        string scope,
        string version,
        params string[] verbs)
        => new()
        {
            resource = resource,
            responseKind = new V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind
            {
                group = "widgets.example.com",
                version = version,
                kind = kind,
            },
            scope = scope,
            verbs = verbs,
        };
}
