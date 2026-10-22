using System.Text.Json;
using System.Text.Json.Nodes;

namespace KubeUI.Kubernetes.Resources.Editor;

/// <summary>
/// Holds the draft JSON tree a resource editor mutates before it is saved.
/// </summary>
public sealed class ResourceEditorDocument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceEditorDocument"/> class.
    /// </summary>
    /// <param name="root">The root object of the draft document.</param>
    public ResourceEditorDocument(JsonObject root)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
        OriginalJson = root.ToJsonString();
    }

    /// <summary>Gets the root object of the draft document.</summary>
    public JsonObject Root { get; }

    /// <summary>Gets the JSON representation of the document as it was loaded.</summary>
    public string OriginalJson { get; }

    /// <summary>Gets a value indicating whether the document differs from its original content.</summary>
    public bool IsDirty => !string.Equals(OriginalJson, Root.ToJsonString(), StringComparison.Ordinal);

    /// <summary>Restores the document to the content it was loaded with.</summary>
    public void Reset()
    {
        var original = JsonNode.Parse(OriginalJson)?.AsObject()
            ?? throw new InvalidOperationException("Original editor document is invalid.");
        Root.Clear();
        foreach (var property in original)
            Root[property.Key] = property.Value?.DeepClone();
    }

    /// <summary>
    /// Parses resource JSON into an editable document.
    /// </summary>
    /// <param name="json">The JSON text of the resource.</param>
    /// <returns>The parsed document.</returns>
    /// <exception cref="JsonException">The JSON root is not an object.</exception>
    public static ResourceEditorDocument Parse(string json)
    {
        var node = JsonNode.Parse(json);
        return node is JsonObject root
            ? new(root)
            : throw new JsonException("Resource JSON must be an object.");
    }
}
