namespace KubeUI.Kubernetes.Serialization;

/// <summary>Application-owned Kubernetes YAML serialization.</summary>
public interface IKubernetesYamlSerializer
{
    /// <summary>Serializes a resource using this application's reflection policy.</summary>
    string Serialize(object value);

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

    /// <summary>Deserializes one resource from an existing YAML parser.</summary>
    object? Deserialize(YamlDotNet.Core.IParser parser, Type type, bool strict = false);

    /// <summary>Deserializes Kubernetes documents, resolving each API version and kind to its model type.</summary>
    List<object> LoadAllFromString(string yaml, Func<string, Type> resolveType, bool strict = false);
}
