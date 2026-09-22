using System.Text.Json.Nodes;
using System.Text.Json;
using KubeUI.Avalonia.Features.Resources.Editor;

namespace KubeUI.Avalonia.Tests.Features.Resources.Editor;

public sealed class ResourceEditorDocumentTests
{
    [Fact]
    public void Parse_PreservesUnknownNestedAndCollectionValues()
    {
        var document = ResourceEditorDocument.Parse("""
            {
              "apiVersion": "example.io/v1",
              "kind": "Widget",
              "metadata": { "name": "demo", "unknown": { "keep": true } },
              "spec": { "items": [1, 2], "labels": { "team": "platform" } }
            }
            """);

        Assert.Equal("demo", document.Root["metadata"]!["name"]!.GetValue<string>());
        Assert.True(document.Root["metadata"]!["unknown"]!["keep"]!.GetValue<bool>());
        Assert.Equal(2, document.Root["spec"]!["items"]!.AsArray().Count);
        Assert.Equal("platform", document.Root["spec"]!["labels"]!["team"]!.GetValue<string>());
        Assert.False(document.IsDirty);
    }

    [Fact]
    public void Reset_RestoresOriginalValuesAndRemovesAddedValues()
    {
        var document = ResourceEditorDocument.Parse("{\"spec\":{\"replicas\":1}}");
        document.Root["spec"]!["replicas"] = 5;
        document.Root["spec"]!["newField"] = "temporary";

        Assert.True(document.IsDirty);

        document.Reset();

        Assert.Equal(1, document.Root["spec"]!["replicas"]!.GetValue<int>());
        Assert.Null(document.Root["spec"]!["newField"]);
        Assert.False(document.IsDirty);
    }

    [Fact]
    public void RootMutation_TracksDirtyStateForScalarCollectionAndMapChanges()
    {
        var document = ResourceEditorDocument.Parse("{\"name\":\"old\",\"items\":[],\"labels\":{}}");

        document.Root["name"] = "new";
        document.Root["items"]!.AsArray().Add(JsonValue.Create(1));
        document.Root["labels"]!["team"] = "platform";

        Assert.True(document.IsDirty);
    }

    [Fact]
    public void Parse_RejectsNonObjectRoot()
    {
        Assert.Throws<JsonException>(() => ResourceEditorDocument.Parse("[1,2,3]"));
    }

}
