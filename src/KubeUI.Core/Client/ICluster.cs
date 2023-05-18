using System.ComponentModel;

namespace KubeUI.Core.Client;

public interface ICluster
{
    string Name { get; set; }

    bool IsConnected { get; }

    bool PodMetrics { get; }

    IEnumerable<T> GetObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    IEnumerable<T> GetObjects<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();

    long CountObjects(string version, string kind, string group = "");

    long CountObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    long CountObjects<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();

    T? GetObject<T>(string version, string kind, string @namespace, string name, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    void Seed<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    void Seed<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();

    void Seed(string version, string kind, string group = "");

    Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>> OnChange;

    Task<V1APIGroupList> GetAPIs();

    IEnumerable<string> GetSelectedNamespaces();

    void SetSelectedNamespaces(IEnumerable<string> namespaces);

    Task<VersionInfo> GetVersion();

    event PropertyChangedEventHandler? PropertyChanged;

    Task ImportYaml(Stream stream);

    Task ImportFolder(string path);

    Task ImportZip(Stream stream);

    PodMetrics? GetPodMetrics(string @namespace, string name);

    Task Connect();
}
