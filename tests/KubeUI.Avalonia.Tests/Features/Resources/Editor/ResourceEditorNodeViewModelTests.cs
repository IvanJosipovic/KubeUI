using System.Text.Json.Nodes;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Editor;
using KubeUI.Kubernetes;
using Microsoft.OpenApi;

namespace KubeUI.Avalonia.Tests.Features.Resources.Editor;

public sealed class ResourceEditorNodeViewModelTests
{
    [Fact]
    public void Create_BuildsNestedObjectArrayAndMapHierarchy()
    {
        var document = ResourceEditorDocument.Parse("""
            {
              "spec": {
                "replicas": 2,
                "containers": [{ "name": "api", "enabled": true }],
                "labels": { "team": "platform" }
              }
            }
            """);

        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), document.Root);

        var spec = Assert.Single(root.Children);
        Assert.Equal(ResourceEditorValueKind.Object, spec.Kind);
        Assert.True(spec.IsSection);
        Assert.Equal(3, spec.Children.Count);

        var containers = Assert.Single(spec.Children, child => child.Name == "containers");
        Assert.Equal(ResourceEditorValueKind.Array, containers.Kind);
        var firstContainer = Assert.Single(containers.Children);
        Assert.Equal("Item 1", firstContainer.DisplayName);
        Assert.Equal(2, firstContainer.Children.Count);

        var labels = Assert.Single(spec.Children, child => child.Name == "labels");
        Assert.Equal(ResourceEditorValueKind.Map, labels.Kind);
        Assert.Equal("platform", Assert.Single(labels.Children).StringValue);
    }

    [Fact]
    public void TypedScalarEdits_UpdateJsonAndExposeValidation()
    {
        var document = ResourceEditorDocument.Parse("""
            { "spec": { "replicas": 2, "containers": [], "labels": {} } }
            """);
        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), document.Root);
        var spec = Assert.Single(root.Children);
        var replicas = Assert.Single(spec.Children, child => child.Name == "replicas");

        replicas.NumberValue = "invalid";

        Assert.NotNull(replicas.ValidationMessage);
        Assert.Equal(2, document.Root["spec"]!["replicas"]!.GetValue<int>());

        replicas.NumberValue = "4";

        Assert.Null(replicas.ValidationMessage);
        Assert.Equal(4m, document.Root["spec"]!["replicas"]!.GetValue<decimal>());
    }

    [Fact]
    public void EnumAndBooleanEdits_UpdateTypedJsonValues()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["mode"] = ScalarSchema("mode", ResourceEditorValueKind.Enum, enumValues: ["Active", "Passive"]),
            ["enabled"] = ScalarSchema("enabled", ResourceEditorValueKind.Boolean),
        });
        var document = ResourceEditorDocument.Parse("{\"mode\":\"Active\",\"enabled\":true}");
        var root = ResourceEditorNodeViewModel.Create(schema, document.Root);

        Assert.Single(root.Children, child => child.Name == "mode").SelectedEnum = "Passive";
        Assert.Single(root.Children, child => child.Name == "enabled").BooleanValue = false;

        var mode = Assert.Single(root.Children, child => child.Name == "mode");
        mode.SelectedEnumOption = Assert.Single(mode.EnumOptions, option => option.Value is null);

        Assert.Null(document.Root["mode"]);
        Assert.False(document.Root["enabled"]!.GetValue<bool>());
    }

    [Fact]
    public void RequiredEnum_DoesNotExposeUnsetOption()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["mode"] = ScalarSchema("mode", ResourceEditorValueKind.Enum, enumValues: ["Active", "Passive"]),
        }, required: new HashSet<string>(["mode"], StringComparer.Ordinal));
        var root = ResourceEditorNodeViewModel.Create(schema, JsonNode.Parse("{\"mode\":\"Active\"}")!);

        var mode = Assert.Single(root.Children);
        Assert.DoesNotContain(mode.EnumOptions, option => option.Value is null);
    }

    [Fact]
    public void Array_AddAndRemoveItem_MutatesCollectionAndRenumbersChildren()
    {
        var document = ResourceEditorDocument.Parse("""
            { "spec": { "replicas": 1, "containers": [], "labels": {} } }
            """);
        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), document.Root);
        var containers = Assert.Single(Assert.Single(root.Children).Children, child => child.Name == "containers");

        containers.AddItem();
        containers.AddItem();

        Assert.Equal(2, containers.Children.Count);
        Assert.Equal("Item 2", containers.Children[1].DisplayName);
        Assert.Equal(2, document.Root["spec"]!["containers"]!.AsArray().Count);

        containers.RemoveChild(containers.Children[0]);

        Assert.Single(containers.Children);
        Assert.Equal("Item 1", containers.Children[0].DisplayName);
        Assert.Single(document.Root["spec"]!["containers"]!.AsArray());
    }

    [Fact]
    public void Map_AddEditAndRemoveEntry_MutatesObject()
    {
        var document = ResourceEditorDocument.Parse("""
            { "spec": { "replicas": 1, "containers": [], "labels": {} } }
            """);
        var root = ResourceEditorNodeViewModel.Create(CreateSchema(), document.Root);
        var labels = Assert.Single(Assert.Single(root.Children).Children, child => child.Name == "labels");

        Assert.True(labels.TryAddMapEntry("team"));
        Assert.False(labels.TryAddMapEntry("team"));
        var team = Assert.Single(labels.Children);
        team.StringValue = "platform";

        Assert.Equal("platform", document.Root["spec"]!["labels"]!["team"]!.GetValue<string>());

        labels.RemoveChild(team);

        Assert.Empty(labels.Children);
        Assert.False(document.Root["spec"]!["labels"]!.AsObject().ContainsKey("team"));
    }

    [Fact]
    public void Create_ExposesDescriptionsAndRequiredMarkers()
    {
        var schema = ObjectSchema(
            "root",
            new Dictionary<string, ResourceEditorSchemaNode>
            {
                ["spec"] = ScalarSchema("spec", ResourceEditorValueKind.String, description: "Desired state"),
            },
            required: new HashSet<string>(["spec"], StringComparer.Ordinal));
        var root = ResourceEditorNodeViewModel.Create(schema, JsonNode.Parse("{\"spec\":\"ready\"}")!);

        var spec = Assert.Single(root.Children);
        Assert.True(spec.IsRequired);
        Assert.Equal("Desired state", spec.Description);
        Assert.Equal("spec *", spec.DisplayName);
    }

    [Fact]
    public void ApiVersionAndKind_AreReadOnly()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["apiVersion"] = ScalarSchema("apiVersion", ResourceEditorValueKind.String),
            ["kind"] = ScalarSchema("kind", ResourceEditorValueKind.String),
        });
        var document = ResourceEditorDocument.Parse("{\"apiVersion\":\"v1\",\"kind\":\"Pod\"}");
        var root = ResourceEditorNodeViewModel.Create(schema, document.Root);
        var apiVersion = Assert.Single(root.Children, child => child.Name == "apiVersion");
        var kind = Assert.Single(root.Children, child => child.Name == "kind");

        apiVersion.StringValue = "apps/v1";
        kind.StringValue = "Deployment";

        Assert.True(apiVersion.IsReadOnly);
        Assert.True(kind.IsReadOnly);
        Assert.Equal("v1", document.Root["apiVersion"]!.GetValue<string>());
        Assert.Equal("Pod", document.Root["kind"]!.GetValue<string>());
    }

    [Fact]
    public void StatusSubtree_IsReadOnlyIncludingNestedCollections()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["status"] = ObjectSchema("status", new Dictionary<string, ResourceEditorSchemaNode>
            {
                ["phase"] = ScalarSchema("phase", ResourceEditorValueKind.String),
                ["conditions"] = new("conditions", ResourceEditorValueKind.Array, null, EmptyProperties,
                    ScalarSchema("condition", ResourceEditorValueKind.String), [], EmptyRequired, null),
            }),
        });
        var document = ResourceEditorDocument.Parse("{\"status\":{\"phase\":\"Running\",\"conditions\":[]}}");
        var root = ResourceEditorNodeViewModel.Create(schema, document.Root);
        var status = Assert.Single(root.Children);
        var phase = Assert.Single(status.Children, child => child.Name == "phase");
        var conditions = Assert.Single(status.Children, child => child.Name == "conditions");

        phase.StringValue = "Failed";
        conditions.AddItemCommand.Execute(null);

        Assert.True(status.IsReadOnly);
        Assert.True(phase.IsReadOnly);
        Assert.True(conditions.IsReadOnly);
        Assert.False(conditions.CanAddItem);
        Assert.Equal("Running", document.Root["status"]!["phase"]!.GetValue<string>());
        Assert.Empty(document.Root["status"]!["conditions"]!.AsArray());
    }

    [Fact]
    public void SchemaMarkedYamlString_UsesYamlEditorValueAndWritesBackAsJsonString()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["values"] = ScalarSchema("values", ResourceEditorValueKind.String, isYamlEditor: true),
        });
        var document = ResourceEditorDocument.Parse("{\"values\":\"{\\\"allow\\\":{\\\"enabled\\\":false}}\"}");
        var root = ResourceEditorNodeViewModel.Create(schema, document.Root);
        var values = Assert.Single(root.Children);

        Assert.True(values.IsYamlText);
        Assert.Contains("allow:", values.YamlValue, StringComparison.Ordinal);

        values.YamlValue = "allow:\n  enabled: true";

        Assert.Contains("\"enabled\":true", document.Root["values"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    [Fact]
    public void JsonLookingStringWithoutYamlSchema_UsesNormalStringEditor()
    {
        var schema = ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["values"] = ScalarSchema("values", ResourceEditorValueKind.String),
        });
        var document = ResourceEditorDocument.Parse("{\"values\":\"{\\\"enabled\\\":true}\"}");
        var values = Assert.Single(ResourceEditorNodeViewModel.Create(schema, document.Root).Children);

        Assert.False(values.IsYamlText);
        Assert.Equal("{\"enabled\":true}", values.StringValue);
    }

    [Fact]
    public void OpenApiPreserveUnknownFields_MarksFieldAsYamlEditor()
    {
        var valuesSchema = new OpenApiSchema { Type = JsonSchemaType.String };
        valuesSchema.UnrecognizedKeywords = new Dictionary<string, JsonNode>
        {
            ["x-kubernetes-preserve-unknown-fields"] = JsonValue.Create(true),
        };
        var specSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema> { ["values"] = valuesSchema },
        };
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["io.helm.toolkit.fluxcd.io.v2.HelmRelease"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema> { ["spec"] = specSchema },
                    },
                },
            },
        };
        var catalog = new KubernetesOpenApiSchemaCatalog();
        catalog.Register(document);

        var schema = ResourceEditorSchemaNode.CreateRoot(
            new GroupApiVersionKind("helm.toolkit.fluxcd.io", "v2", "HelmRelease", "helmreleases"), catalog);

        Assert.True(schema.Properties["spec"].Properties["values"].IsYamlEditor);
    }

    private static ResourceEditorSchemaNode CreateSchema()
    {
        var container = ObjectSchema("container", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["name"] = ScalarSchema("name", ResourceEditorValueKind.String),
            ["enabled"] = ScalarSchema("enabled", ResourceEditorValueKind.Boolean),
        });
        var spec = ObjectSchema("spec", new Dictionary<string, ResourceEditorSchemaNode>
        {
            ["replicas"] = ScalarSchema("replicas", ResourceEditorValueKind.Number),
            ["containers"] = new("containers", ResourceEditorValueKind.Array, null, EmptyProperties, container, [], EmptyRequired, "Container list"),
            ["labels"] = new("labels", ResourceEditorValueKind.Map, null, EmptyProperties, ScalarSchema("value", ResourceEditorValueKind.String), [], EmptyRequired, "Labels"),
        });
        return ObjectSchema("root", new Dictionary<string, ResourceEditorSchemaNode> { ["spec"] = spec });
    }

    private static ResourceEditorSchemaNode ObjectSchema(
        string name,
        IReadOnlyDictionary<string, ResourceEditorSchemaNode> properties,
        IReadOnlySet<string>? required = null)
        => new(name, ResourceEditorValueKind.Object, null, properties, null, [], required ?? EmptyRequired, null);

    private static ResourceEditorSchemaNode ScalarSchema(
        string name,
        ResourceEditorValueKind kind,
        IReadOnlyList<string>? enumValues = null,
        string? description = null,
        bool isYamlEditor = false)
        => new(name, kind, null, EmptyProperties, null, enumValues ?? [], EmptyRequired, description, isYamlEditor);

    private static readonly IReadOnlyDictionary<string, ResourceEditorSchemaNode> EmptyProperties =
        new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> EmptyRequired = new HashSet<string>(StringComparer.Ordinal);
}
