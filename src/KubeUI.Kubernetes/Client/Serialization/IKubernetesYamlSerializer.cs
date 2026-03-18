namespace KubeUI.Kubernetes.Serialization;

public interface IKubernetesYamlSerializer
{
    string Serialize(object value);
}

