using System.Collections.Frozen;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>Describes one served resource endpoint from Kubernetes API discovery.</summary>
public sealed record ApiDiscoveryResource
{
    /// <summary>Creates a discovered endpoint descriptor.</summary>
    /// <param name="kind">Group, version, kind, and plural resource name.</param>
    /// <param name="isNamespaced">Whether the endpoint is namespace-scoped.</param>
    /// <param name="freshness">Discovery freshness reported for the API version.</param>
    /// <param name="verbs">Verbs advertised by discovery. These do not grant authorization.</param>
    public ApiDiscoveryResource(
        GroupApiVersionKind kind,
        bool isNamespaced,
        string freshness,
        IEnumerable<string> verbs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind.Kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind.PluralName);
        ArgumentNullException.ThrowIfNull(verbs);

        Kind = kind;
        IsNamespaced = isNamespaced;
        Freshness = freshness ?? string.Empty;
        Verbs = verbs.ToFrozenSet(StringComparer.Ordinal);
    }

    /// <summary>Gets the resource group, version, kind, and plural name.</summary>
    public GroupApiVersionKind Kind { get; }

    /// <summary>Gets whether the endpoint is namespace-scoped.</summary>
    public bool IsNamespaced { get; }

    /// <summary>Gets discovery freshness for the API version.</summary>
    public string Freshness { get; }

    /// <summary>Gets verbs advertised by discovery; this set is not an authorization result.</summary>
    public IReadOnlySet<string> Verbs { get; }
}
