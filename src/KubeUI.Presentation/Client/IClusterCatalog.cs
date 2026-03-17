namespace KubeUI.Client;

public interface IClusterCatalog
{
    IEnumerable<ICluster> Clusters { get; }
    ICluster? GetCluster(string name);
    ICluster? GetDefault();
    void LoadFromConfigFromPath(string path);
    void RemoveCluster(ICluster cluster);
}
