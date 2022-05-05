using k8s;
using k8s.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace KubeCRDGenerator;

public class GenericObject : IKubernetesObject<V1ObjectMeta>, ISpec<JsonObject>
{
    public string ApiVersion { get; set; }
    public string Kind { get; set; }
    public V1ObjectMeta Metadata { get; set; }
    public GenericObjectStatus? Status { get; set; }
    public JsonObject Spec { get; set; }
}

public class GenericObjectStatus
{
    [JsonPropertyName("conditions")]
    public IList<V1CustomResourceDefinitionCondition>? Conditions { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; set; }
}

public class GenericObjectCondition
{
    //
    // Summary:
    //     lastTransitionTime last time the condition transitioned from one status to another.
    [JsonPropertyName("lastTransitionTime")]
    public DateTime? LastTransitionTime { get; set; }
    //
    // Summary:
    //     message is a human-readable message indicating details about last transition.
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    //
    // Summary:
    //     reason is a unique, one-word, CamelCase reason for the condition's last transition.
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    //
    // Summary:
    //     status is the status of the condition. Can be True, False, Unknown.
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    //
    // Summary:
    //     type is the type of the condition. Types include Established, NamesAccepted and
    //     Terminating.
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
