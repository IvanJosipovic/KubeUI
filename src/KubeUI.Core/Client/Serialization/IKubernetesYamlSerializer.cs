namespace KubeUI.Client.Serialization;

public interface IKubernetesYamlSerializer
{
    string Serialize(object value);
}
