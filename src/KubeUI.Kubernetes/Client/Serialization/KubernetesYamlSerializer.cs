using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubernetesYamlSerializer : IKubernetesYamlSerializer
{
    public string Serialize(object value)
    {
        return KubernetesYaml.Serialize(value);
    }
}


