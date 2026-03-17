using KubeUI.Client.Serialization;

namespace KubeUI.Client.Serialization;

internal sealed class KubernetesYamlSerializer : IKubernetesYamlSerializer
{
    public string Serialize(object value)
    {
        return KubernetesYaml.Serialize(value);
    }
}
