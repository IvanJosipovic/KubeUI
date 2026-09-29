using System.Text.Json;
using System.Text.Json.Serialization;
using KubeUI.Avalonia.Infrastructure.Mcp;

namespace KubeUI.Desktop;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(IReadOnlyList<McpClusterInfo>))]
[JsonSerializable(typeof(McpClusterInfo))]
[JsonSerializable(typeof(IReadOnlyList<McpSupportedResourceInfo>))]
[JsonSerializable(typeof(IReadOnlyList<McpResourceInfo>))]
[JsonSerializable(typeof(IReadOnlyList<McpRelatedResourceInfo>))]
[JsonSerializable(typeof(McpResourceGraphInfo))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(bool))]
internal partial class McpToolJsonSerializationContext : JsonSerializerContext;
