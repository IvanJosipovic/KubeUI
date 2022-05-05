using k8s;
using k8s.Models;

namespace KubeUI.Core.Client;

public interface ICluster
{
    string Name { get; set; }

    bool IsConnected { get; set; }

    IEnumerable<T> GetObjects<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    IEnumerable<T> GetObjects<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();

    T? GetObject<T>(string version, string kind, string @namespace, string name, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    void Seed<T>(string version, string kind, string group = "") where T : class, IKubernetesObject<V1ObjectMeta>, new();

    void Seed<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new();

    void AddObject(IKubernetesObject<V1ObjectMeta> @object);

    void AddObjects(IEnumerable<IKubernetesObject<V1ObjectMeta>> objects);

    Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>> OnChange;

    public Type? GetResourceType(GroupApiVersionKind type);

    public Type? GetResourceType(string group, string version, string kind);

    Task<V1APIGroupList> GetAPIs();

    IEnumerable<string> GetSelectedNamespaces();

    void SetSelectedNamespaces(IEnumerable<string> namespaces);

    Task<KubeVersion> GetVersion();
}
