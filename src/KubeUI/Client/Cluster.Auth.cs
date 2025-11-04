using k8s;
using k8s.Models;
using Swordfish.NET.Collections;
using Yarp.Kubernetes.Controller.Client;

namespace KubeUI.Client;

public partial class Cluster
{
    private ConcurrentObservableCollection<V1SelfSubjectAccessReview> _selfSubjectAccessReviews { get; } = [];

    [ObservableProperty]
    public partial bool ListNamespaces { get; set; }

    private async Task GetPermissions()
    {
        ListNamespaces = await UpdateCanIAnyNamespaceAsync<V1Namespace>(Verb.List) && await UpdateCanIAnyNamespaceAsync<V1Namespace>(Verb.Watch);
    }

    public enum Verb
    {
        Create,
        Delete,
        Get,
        List,
        Patch,
        Update,
        Watch,
        DeleteCollection
    }

    private async Task GetSelfSubjectAccessReview(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        var kind = GroupApiVersionKind.From(type);

        var review = _selfSubjectAccessReviews.ToList().FirstOrDefault(x =>
            x.Spec.ResourceAttributes.Group == (string.IsNullOrEmpty(kind.Group) ? null : kind.Group) &&
            x.Spec.ResourceAttributes.NamespaceProperty == (string.IsNullOrEmpty(@namespace) ? null : @namespace) &&
            x.Spec.ResourceAttributes.Resource == kind.PluralName &&
            x.Spec.ResourceAttributes.Subresource == (string.IsNullOrEmpty(subresource) ? null : subresource) &&
            x.Spec.ResourceAttributes.Verb == verb.ToString().ToLowerInvariant() &&
            x.Spec.ResourceAttributes.Version == kind.ApiVersion
        );

        if (review != null)
        {
            return;
        }

        var model = new V1SelfSubjectAccessReview()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = (string.IsNullOrEmpty(kind.Group) ? "" : kind.Group),
                    NamespaceProperty = (string.IsNullOrEmpty(@namespace) ? "" : @namespace),
                    Resource = kind.PluralName,
                    Subresource = (string.IsNullOrEmpty(subresource) ? "" : subresource),
                    Verb = verb.ToString().ToLowerInvariant(),
                    Version = kind.ApiVersion
                }
            }
        };

        var resp = await Client.AuthorizationV1.CreateSelfSubjectAccessReviewAsync(model);

        _selfSubjectAccessReviews.Add(resp);
    }

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        var kind = GroupApiVersionKind.From(type);

        // If checking namespace permissions, we should check if we have cluster level first
        if (!string.IsNullOrEmpty(@namespace))
        {
            var global = _selfSubjectAccessReviews.ToList().FirstOrDefault(x =>
                x.Spec.ResourceAttributes.Verb == verb.ToString().ToLowerInvariant() &&
                x.Spec.ResourceAttributes.Resource == kind.PluralName &&
                x.Spec.ResourceAttributes.Group == (string.IsNullOrEmpty(kind.Group) ? null : kind.Group) &&
                x.Spec.ResourceAttributes.NamespaceProperty == null &&
                x.Spec.ResourceAttributes.Subresource == (string.IsNullOrEmpty(subresource) ? null : subresource) &&
                x.Spec.ResourceAttributes.Version == kind.ApiVersion
            );

            if (global?.Status.Allowed == true)
            {
                return true;
            }
        }

        var review = _selfSubjectAccessReviews.ToList().FirstOrDefault(x =>
            x.Spec.ResourceAttributes.Verb == verb.ToString().ToLowerInvariant() &&
            x.Spec.ResourceAttributes.Resource == kind.PluralName &&
            x.Spec.ResourceAttributes.Group == (string.IsNullOrEmpty(kind.Group) ? null : kind.Group) &&
            x.Spec.ResourceAttributes.NamespaceProperty == (string.IsNullOrEmpty(@namespace) ? null : @namespace) &&
            x.Spec.ResourceAttributes.Subresource == (string.IsNullOrEmpty(subresource) ? null : subresource) &&
            x.Spec.ResourceAttributes.Version == kind.ApiVersion
        );

        if (review == null)
        {
            var error = string.Format("Missing V1SelfSubjectAccessReview {0} {1}/{2}/{3}", verb, kind.Group, kind.PluralName, subresource);

            _logger.LogCritical(error);
            throw new Exception(error);
        }

        return review?.Status.Allowed == true;
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CanI(typeof(T), verb, @namespace, subresource);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        if (CanI(type, verb, subresource: subresource))
        {
            return true;
        }

        if (IsNamespaced(type))
        {
            foreach (var item in GetObjectDictionary<V1Namespace>())
            {
                if (CanI(type, verb, item.Value.Name(), subresource))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CanIAnyNamespace(typeof(T), verb, subresource);
    }

    public async Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        await GetSelfSubjectAccessReview(type, verb, subresource: subresource);

        if (CanI(type, verb, subresource: subresource))
        {
            return true;
        }

        if (IsNamespaced(type))
        {
            foreach (var item in await GetObjectDictionaryAsync<V1Namespace>())
            {
                await GetSelfSubjectAccessReview(type, verb, item.Value.Name(), subresource);

                if (CanI(type, verb, item.Value.Name(), subresource))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await UpdateCanIAnyNamespaceAsync(typeof(T), verb, subresource);
    }

    public async Task UpdateCanIAllNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        await GetSelfSubjectAccessReview(type, verb, subresource: subresource);

        if (IsNamespaced(type))
        {
            var tasks = new List<Task>();

            foreach (var item in await GetObjectDictionaryAsync<V1Namespace>())
            {
                tasks.Add(GetSelfSubjectAccessReview(type, verb, item.Value.Name(), subresource));
            }

            await Task.WhenAll(tasks);
        }
    }

    public async Task UpdateCanIAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await UpdateCanIAllNamespaceAsync(typeof(T), verb, subresource);
    }
}
