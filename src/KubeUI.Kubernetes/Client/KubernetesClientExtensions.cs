using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public static class KubernetesClientExtensions
{
    public static GenericClient GetGenericClient<T>(this IKubernetes client) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();
        return new GenericClient(client, api.Group, api.ApiVersion, api.PluralName, false);
    }

    public static GenericClient GetGenericClient(this IKubernetes client, IKubernetesObject<V1ObjectMeta> item)
    {
        var api = GroupApiVersionKind.From(item.GetType());
        return new GenericClient(client, api.Group, api.ApiVersion, api.PluralName, false);
    }
}

