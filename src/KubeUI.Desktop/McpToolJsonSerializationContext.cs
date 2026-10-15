using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Desktop;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(IReadOnlyList<McpClusterInfo>))]
[JsonSerializable(typeof(McpClusterInfo))]
[JsonSerializable(typeof(IReadOnlyList<McpSupportedResourceInfo>))]
[JsonSerializable(typeof(McpResourcePage))]
[JsonSerializable(typeof(McpEventPage))]
[JsonSerializable(typeof(IReadOnlyList<McpResourceInfo>))]
[JsonSerializable(typeof(IReadOnlyList<McpRelatedResourceInfo>))]
[JsonSerializable(typeof(McpResourceGraphInfo))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(bool))]
internal partial class McpToolJsonSerializationContext : JsonSerializerContext
{
    /// <summary>
    /// Creates the serializer options for MCP tool results and schemas. Tool results can embed
    /// Kubernetes objects such as <see cref="k8s.Models.Corev1Event"/>, so the Kubernetes
    /// source-generated context contributes the additional model metadata.
    /// </summary>
    /// <returns>Options combining the MCP tool and Kubernetes generated metadata.</returns>
    internal static JsonSerializerOptions CreateSerializerOptions()
        => new(Default.Options)
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                Default,
                KubernetesJsonStaticContext.Default)
        };
}
