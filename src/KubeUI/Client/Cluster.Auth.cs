using k8s;
using k8s.Models;
using Swordfish.NET.Collections;

namespace KubeUI.Client;

public partial class Cluster
{
    private ConcurrentObservableCollection<V1SelfSubjectAccessReview> _selfSubjectAccessReviews { get; } = [];

    [ObservableProperty]
    public partial bool ListNamespaces { get; set; }

    private async Task GetPermissions()
    {
        ListNamespaces = await UpdateCanIListWatchAnyNamespaceAsync<V1Namespace>();
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

    private async Task GetSelfSubjectAccessReview(Type type, Verb verb, string @namespace = "", string subresource = "")
    {
        var kind = GroupApiVersionKind.From(type);
        var kube = Client as Kubernetes;

        var review = _selfSubjectAccessReviews.ToList().FirstOrDefault(x =>
            x.Spec.ResourceAttributes.Verb == verb.ToString().ToLowerInvariant() &&
            x.Spec.ResourceAttributes.Resource == kind.PluralName &&
            x.Spec.ResourceAttributes.Group == (string.IsNullOrEmpty(kind.Group) ? null : kind.Group) &&
            x.Spec.ResourceAttributes.NamespaceProperty == (string.IsNullOrEmpty(@namespace) ? null : @namespace) &&
            x.Spec.ResourceAttributes.Subresource == (string.IsNullOrEmpty(subresource) ? null : subresource)
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
                    Group = kind.Group,
                    NamespaceProperty = @namespace,
                    Resource = kind.PluralName,
                    Subresource = subresource,
                    Verb = verb.ToString().ToLowerInvariant()
                }
            }
        };

        var resp = await kube.CreateSelfSubjectAccessReviewAsync(model);

         _selfSubjectAccessReviews.Add(resp);
    }

    private async Task GetSelfSubjectAccessReview<T>(Verb verb, string @namespace = "", string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await GetSelfSubjectAccessReview(typeof(T), verb, @namespace, subresource);
    }

    public bool CanI(Type type, Verb verb, string @namespace = "", string subresource = "")
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
                x.Spec.ResourceAttributes.Subresource == (string.IsNullOrEmpty(subresource) ? null : subresource)
            );

            if (global != null && global.Status.Allowed)
            {
                return true;
            }
        }

        var review = _selfSubjectAccessReviews.ToList().FirstOrDefault(x =>
            x.Spec.ResourceAttributes.Verb == verb.ToString().ToLowerInvariant() &&
            x.Spec.ResourceAttributes.Resource == kind.PluralName &&
            x.Spec.ResourceAttributes.Group == (string.IsNullOrEmpty(kind.Group) ? null : kind.Group) &&
            x.Spec.ResourceAttributes.NamespaceProperty == (string.IsNullOrEmpty(@namespace) ? null : @namespace) &&
            x.Spec.ResourceAttributes.Subresource == (string.IsNullOrEmpty(subresource) ? null : subresource)
        );

        if (review == null)
        {
            _logger.LogCritical("Missing V1SelfSubjectAccessReview {verb} {group}/{resource}/{subresource}", verb, kind.Group, kind.PluralName, subresource);
        }

        return review?.Status.Allowed == true;
    }

    public bool CanI<T>(Verb verb, string @namespace = "", string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CanI(typeof(T), verb, @namespace, subresource);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string subresource = "")
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

    public async Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string subresource = "")
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

    public async Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await UpdateCanIAnyNamespaceAsync(typeof(T), verb, subresource);
    }

    private async Task<bool> UpdateCanListWatchAnyNamespaceAsync(Type type, string subresource = "")
    {
        return await UpdateCanIAnyNamespaceAsync(type, Verb.List, subresource) && await UpdateCanIAnyNamespaceAsync(type, Verb.Watch, subresource);
    }

    private async Task<bool> UpdateCanIListWatchAnyNamespaceAsync<T>(string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await UpdateCanListWatchAnyNamespaceAsync(typeof(T), subresource);
    }
}
