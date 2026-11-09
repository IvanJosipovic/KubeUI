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
                    ],
                },
            ],
        };

        var snapshot = ApiDiscoverySnapshot.Create(core, groups);

        snapshot.Resources.Count.ShouldBe(2);
        snapshot.Resources.ShouldNotContain(resource => resource.Kind.PluralName == "widgets/status");
        snapshot.PreferredResources.ShouldHaveSingleItem();
        snapshot.PreferredResources[0].Kind.ShouldBe(new GroupApiVersionKind("widgets.example.com", "v1", "Widget", "widgets"));
        snapshot.PreferredResources[0].IsNamespaced.ShouldBeTrue();
        snapshot.PreferredResources[0].Verbs.ShouldContain("list");
    }

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
