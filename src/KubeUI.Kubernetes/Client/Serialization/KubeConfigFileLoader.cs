using k8s;
using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes.Serialization;

internal static class KubeConfigFileLoader
{
    public static K8SConfiguration Load(string path, IKubernetesYamlSerializer yamlSerializer, bool useRelativePaths = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(yamlSerializer);

        var configuration = yamlSerializer.Deserialize<K8SConfiguration>(File.ReadAllText(path));
        if (useRelativePaths)
        {
            configuration.FileName = Path.GetFullPath(path);
        }

        return configuration;
    }
}
