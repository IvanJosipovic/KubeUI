using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships;

/// <summary>Selects resources whose annotations match every supplied key/value pair.</summary>
public sealed class ResourceAnnotationSelector
{
    private readonly KeyValuePair<string, string>[] _annotations;

    /// <summary>Creates a selector with an immutable snapshot of the required annotations.</summary>
    /// <param name="annotations">Required annotation values; at least one must be provided.</param>
    public ResourceAnnotationSelector(IReadOnlyDictionary<string, string> annotations)
    {
        ArgumentNullException.ThrowIfNull(annotations);
        if (annotations.Count == 0)
        {
            throw new ArgumentException("At least one annotation is required.", nameof(annotations));
        }

        foreach (var (key, value) in annotations)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
        }

        _annotations = annotations.ToArray();
    }

    /// <summary>Returns whether the resource satisfies all required annotation values.</summary>
    /// <param name="resource">Resource to match.</param>
    public bool Matches(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.Metadata?.Annotations is not { } annotations)
        {
            return false;
        }

        foreach (var (key, value) in _annotations)
        {
            if (!annotations.TryGetValue(key, out var actual) || !string.Equals(actual, value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
