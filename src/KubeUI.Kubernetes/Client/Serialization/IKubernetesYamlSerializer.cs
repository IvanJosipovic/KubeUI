using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Serialization;

/// <summary>Application-owned Kubernetes YAML serialization.</summary>
public interface IKubernetesYamlSerializer
{
    /// <summary>Serializes a resource using this application's reflection policy.</summary>
    string Serialize(object value);

    /// <summary>Serializes Kubernetes resources as a multi-document YAML stream.</summary>
    string SerializeAll(IEnumerable<object> values);

    /// <summary>Serializes a resource directly to a writer using this application's reflection policy.</summary>
    /// <remarks>Implementations can override this method to stream output without creating an intermediate string.</remarks>
    void Serialize(object value, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.Write(Serialize(value));
    }
    /// <summary>Deserializes a resource using this application's reflection policy.</summary>
    T Deserialize<T>(string yaml, bool strict = false);

    /// <summary>Deserializes a runtime resource type using this application's reflection policy.</summary>
    object? Deserialize(string yaml, Type type, bool strict = false);

    /// <summary>Deserializes Kubernetes resources, resolving each API version and kind to its model type.</summary>
    List<IKubernetesObject<V1ObjectMeta>> LoadAllFromString(string yaml, Func<string, Type> resolveType, bool strict = false);

    /// <summary>Deserializes every document directly as a generic Kubernetes resource.</summary>
    List<GenericKubernetesObject> LoadGenericResourcesFromString(string yaml, bool strict = false);
}
