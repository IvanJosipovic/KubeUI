using k8s;
using k8s.Models;
using Swordfish.NET.Collections;

namespace KubeUI.Client;

public partial class Cluster
{
    private ConcurrentObservableCollection<V1SelfSubjectAccessReview> _selfSubjectAccessReviews { get; } = [];

    private bool _listNamspaces;

    private async Task GetPermissions()
    {
        _listNamspaces = await CanIListWatchAsync<V1Namespace>();
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

        if (resp.Spec == null)
        {
            throw new Exception("V1SelfSubjectAccessReview is null");
        }

         _selfSubjectAccessReviews.Add(resp);
    }

    private async Task GetSelfSubjectAccessReview<T>(Verb verb, string @namespace = "", string subresource = "") where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await GetSelfSubjectAccessReview(typeof(T), verb, @namespace, subresource);
    }

    public bool CanI(Type type, Verb verb, string @namespace = "", string subresource = "")
    {
        var kind = GroupApiVersionKind.From(type);

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
        return CanI(typeof(T), verb, subresource);
    }

    public async Task<bool> CanIAsync(Type type, Verb verb, string subresource = "", bool checkNamespace = false)
    {
        await GetSelfSubjectAccessReview(type, Verb.List, subresource: subresource);

        if (CanI(type, verb, subresource))
        {
            return true;
        }

        if (checkNamespace)
        {
            foreach (var item in await GetObjectDictionaryAsync<V1Namespace>())
            {
                await GetSelfSubjectAccessReview(type, verb, subresource, item.Value.Name());

                if (CanI(type, verb, subresource, item.Value.Name()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<bool> CanIAsync<T>(Verb verb, string subresource = "", bool checkNamespace = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await CanIAsync(typeof(T), verb, subresource, checkNamespace);
    }

    private async Task<bool> CanIListWatchAsync<T>(bool checkNamespace = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await CanListWatchAsync(typeof(T), checkNamespace);
    }

    private async Task<bool> CanListWatchAsync(Type type, bool checkNamespace = false)
    {
        await Task.WhenAll(
            GetSelfSubjectAccessReview(type, Verb.List),
            GetSelfSubjectAccessReview(type, Verb.Watch)
        );

        if (CanI(type, Verb.List) && CanI(type, Verb.Watch))
        {
            return true;
        }

        if (checkNamespace)
        {
            foreach (var item in await GetObjectDictionaryAsync<V1Namespace>())
            {
                await Task.WhenAll(
                    GetSelfSubjectAccessReview(type, Verb.List, item.Value.Name()),
                    GetSelfSubjectAccessReview(type, Verb.Watch, item.Value.Name())
                    );

                if (CanI(type, Verb.List, item.Value.Name()) && CanI(type, Verb.Watch, item.Value.Name()))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
