using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public static class KubernetesClientExtensions
{
    public static async Task<HttpResponseMessage> SendAuthenticatedAsync(
        this k8s.Kubernetes client,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        if (client.Credentials is not null)
        {
            await client.Credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return await client.HttpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the served storage version of a custom resource definition.
    /// </summary>
    /// <param name="definition">The custom resource definition.</param>
    /// <param name="kind">The resource group, version, kind, and plural name.</param>
    /// <returns><see langword="true"/> when the definition has a usable version.</returns>
    public static bool TryGetResourceKind(
        this V1CustomResourceDefinition definition,
        out GroupApiVersionKind kind)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var spec = definition.Spec;
        var version = spec?.Versions?.FirstOrDefault(candidate => candidate.Served && candidate.Storage);
        if (spec?.Names is null || version is null)
        {
            kind = default;
            return false;
        }

        kind = new GroupApiVersionKind(spec.Group, version.Name, spec.Names.Kind, spec.Names.Plural);
        return true;
    }

    /// <summary>
    /// Resolves the API identity for a typed resource or a registered custom resource instance.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="item">The resource instance.</param>
    /// <param name="modelCatalog">The catalog used to resolve custom resource kinds.</param>
    /// <returns>The resource group, version, kind, and plural name.</returns>
    public static GroupApiVersionKind GetResourceKind<T>(
        this T item,
        ClusterModelCatalog modelCatalog)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(modelCatalog);

        if (item is GenericKubernetesObject genericItem
            && modelCatalog.TryGetResourceKind(genericItem, out var genericKind))
        {
            return genericKind;
        }

        if (item is GenericKubernetesObject unresolvedGenericItem)
        {
            throw new InvalidOperationException(
                $"Unable to resolve resource kind for {unresolvedGenericItem.ApiVersion}/{unresolvedGenericItem.Kind}.");
        }

        return GroupApiVersionKind.From<T>();
    }

    public static GenericClient GetGenericClient<T>(this IKubernetes client) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();
        return new GenericClient(client, api.Group, api.ApiVersion, api.PluralName, false);
    }

    /// <summary>
    /// Creates a generic client for a typed resource or a registered custom resource instance.
    /// </summary>
    /// <param name="client">The Kubernetes client.</param>
    /// <param name="item">The resource instance.</param>
    /// <param name="modelCatalog">The catalog used to resolve custom resource kinds.</param>
    /// <returns>A generic client for the resource API.</returns>
    public static GenericClient GetGenericClient<T>(
        this IKubernetes client,
        T item,
        ClusterModelCatalog modelCatalog)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return client.GetGenericClient(item.GetResourceKind(modelCatalog));
    }

    public static GenericClient GetGenericClient(this IKubernetes client, GroupApiVersionKind api)
    {
        return new GenericClient(client, api.Group, api.ApiVersion, api.PluralName, false);
    }
}
