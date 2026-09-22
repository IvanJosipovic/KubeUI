using System.Text.Json;
using System.Text.Json.Serialization;

using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

public class GenericKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public V1ObjectMeta Metadata { get; set; } = default!;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
