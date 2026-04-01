namespace KubeUI.Kubernetes;

public interface IClusterSettingsStore
{
    IReadOnlyCollection<string> KubeConfigPaths { get; }

    void AddKubeConfigPath(string path);

    IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster);
}
