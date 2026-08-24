using k8s.Models;
using KubernetesClient.Informer.Client;
using Microsoft.OpenApi;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Models;

public sealed class ClusterModelCatalogTests
{
    [Fact]
    public void ModelCatalogStartsEmpty()
    {
        var catalog = new KubernetesModelCatalog();

        catalog.TryGetResourceKind("example.com", "v1", "Widget", out _).ShouldBeFalse();
    }

    [Fact]
    public void BuiltInModelLookupsAreAvailableThroughClusterModelCatalog()
    {
        var sharedCatalog = CreateCatalogWithPod();
        var catalog = new ClusterModelCatalog(sharedCatalog);

        catalog.TryGetResourceKind("v1", "Pod", out var podKind).ShouldBeTrue();
        podKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
        catalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.core.v1.Pod"] = new OpenApiSchema { Description = "Pod description" },
                },
            },
        });
        catalog.OpenApiSchemas
            .GetDescription(GroupApiVersionKind.From<V1Pod>())
            .ShouldBe("Pod description");
    }

    [Fact]
    public void ClusterCatalogsShareBuiltInCatalogButKeepSeparateCrdCaches()
    {
        var sharedCatalog = new KubernetesModelCatalog();
        var first = new ClusterModelCatalog(sharedCatalog);
        var second = new ClusterModelCatalog(sharedCatalog);

        var kind = new GroupApiVersionKind("example.com", "v1", "Widget", "widgets");
        first.RegisterCustomResourceDefinition(kind);
        first.IsCustomResource(kind).ShouldBeTrue();
        second.IsCustomResource(kind).ShouldBeFalse();
    }

    [Fact]
    public void CustomResourceKindsDoNotResolveToClrModels()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        var kind = new GroupApiVersionKind("example.com", "v1beta1", "Widget", "widgets");

        catalog.RegisterCustomResourceDefinition(kind);

        catalog.Contains(kind).ShouldBeTrue();
        catalog.IsCustomResource(kind).ShouldBeTrue();
    }

    [Fact]
    public void ResourceKindsResolveWithoutClrModels()
    {
        var catalog = new ClusterModelCatalog(CreateCatalogWithPod());
        var customKind = new GroupApiVersionKind("example.com", "v1beta1", "Widget", "widgets");

        catalog.RegisterCustomResourceDefinition(customKind);

        catalog.TryGetResourceKind("v1", "Pod", out var podKind).ShouldBeTrue();
        podKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
        catalog.TryGetResourceKind("example.com/v1beta1", "Widget", out var resolvedCustomKind).ShouldBeTrue();
        resolvedCustomKind.ShouldBe(customKind);
    }

    [Fact]
    public void ResourcePayloadResolvesWithoutInspectingClrType()
    {
        var catalog = new ClusterModelCatalog(CreateCatalogWithPod());
        var pod = new V1Pod
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
        };

        catalog.TryGetResourceKind(pod, out var resourceKind).ShouldBeTrue();
        resourceKind.ShouldBe(GroupApiVersionKind.From<V1Pod>());
    }

    private static KubernetesModelCatalog CreateCatalogWithPod()
    {
        var catalog = new KubernetesModelCatalog();
        catalog.Register(GroupApiVersionKind.From<V1Pod>(), typeof(V1Pod));
        return catalog;
    }

    [Fact]
    public void OpenApiSchemaLookupUsesGroupVersionKindSuffix()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        catalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.example.v2beta1.Widget"] = new OpenApiSchema { Description = "Widget" },
                },
            },
        });

        catalog.OpenApiSchemas.GetDescription(
            new GroupApiVersionKind("example", "v2beta1", "Widget", "widgets"))
            .ShouldBe("Widget");
    }

    [Fact]
    public void OpenApiSchemaLookupDoesNotUseAnUnrelatedKindFallback()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        catalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.k8s.api.apps.v1.Widget"] = new OpenApiSchema { Description = "Apps widget" },
                },
            },
        });

        catalog.OpenApiSchemas
            .GetSchema(new GroupApiVersionKind("example.com", "v1", "Widget", "widgets"))
            .ShouldBeNull();
    }

    [Fact]
    public void OpenApiSchemaLookupSupportsReversedDnsCustomResourceNames()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        var schema = new OpenApiSchema { Description = "CloudNativePG Cluster" };
        catalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.cnpg.postgresql.v1.Cluster"] = schema,
                },
            },
        });

        var kind = new GroupApiVersionKind("postgresql.cnpg.io", "v1", "Cluster", "clusters");

        catalog.OpenApiSchemas
            .GetSchema(kind)
            .ShouldBeSameAs(schema);
    }

    [Fact]
    public void OpenApiSchemaLookupSupportsLegacyGroupedNames()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        var schema = new OpenApiSchema { Description = "Legacy widget" };
        catalog.RegisterOpenApiSchema(new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.example.com.v1.Widget"] = schema,
                },
            },
        });

        catalog.OpenApiSchemas
            .GetSchema(new GroupApiVersionKind("example.com", "v1", "Widget", "widgets"))
            .ShouldBeSameAs(schema);
    }

    [Fact]
    public void OpenApiSchemaLookupExpandsSchemaReferences()
    {
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>(),
            },
        };
        var target = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["name"] = new OpenApiSchema { Type = JsonSchemaType.String },
            },
        };
        document.Components.Schemas["Target"] = target;
        document.RegisterComponents();
        document.Components.Schemas["Root"] = new OpenApiSchemaReference("Target", document);

        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        catalog.RegisterOpenApiSchema(document);

        var schema = catalog.OpenApiSchemas.GetSchema("Root");

        schema.ShouldBeSameAs(target);
        schema.Properties.ShouldContainKey("name");
    }

    [Fact]
    public void OpenApiSchemaLookupResolvesReferencesAcrossRegisteredDocuments()
    {
        var reference = new OpenApiSchemaReference("Target", null, "#/components/schemas/Target");
        var rootDocument = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["Root"] = new OpenApiSchema
                    {
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["spec"] = reference,
                        },
                    },
                },
            },
        };
        var targetDocument = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["Target"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>(),
                    },
                },
            },
        };

        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        catalog.RegisterOpenApiSchema(rootDocument);
        catalog.RegisterOpenApiSchema(targetDocument);

        catalog.OpenApiSchemas.ExpandReferences(catalog.OpenApiSchemas.GetSchema("Root")!.Properties["spec"])
            .ShouldBeSameAs(catalog.OpenApiSchemas.GetSchema("Target"));
    }
}
