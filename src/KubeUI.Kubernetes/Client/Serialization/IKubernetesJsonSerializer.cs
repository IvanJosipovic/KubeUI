namespace KubeUI.Kubernetes.Serialization;

/// <summary>Application-owned Kubernetes JSON serialization configuration.</summary>
public interface IKubernetesJsonSerializer
{
    /// <summary>Applies this application's JSON policy to a Kubernetes client configuration.</summary>
    void ConfigureClient(k8s.KubernetesClientConfiguration configuration);

    /// <summary>Serializes a Kubernetes resource using this application's reflection policy.</summary>
    string Serialize(object value);

    /// <summary>Deserializes a Kubernetes resource using this application's reflection policy.</summary>
    T Deserialize<T>(string json);
}
