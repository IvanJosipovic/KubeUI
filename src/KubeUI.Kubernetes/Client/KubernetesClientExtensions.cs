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

    public static GenericClient GetGenericClient<T>(this IKubernetes client) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();
        return new GenericClient(client, api.Group, api.ApiVersion, api.PluralName, false);
    }

    public static GenericClient GetGenericClient(this IKubernetes client, GroupApiVersionKind api)
    {
        return new GenericClient(client, api.Group, api.ApiVersion, api.PluralName, false);
    }
}
