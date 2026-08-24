using System.Text.Json;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class CrossplaneUsageRelationshipProvider : IResourceRelationshipProvider
{
    private const string UsageApiGroup = "protection.crossplane.io";
    private const string UsageKind = "Usage";

    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(new(UsageApiGroup, "v1beta1", UsageKind, "usages"), allowServedVersionFallback: true),
    ];

    public void AddRelationships(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        if (!string.Equals(GetApiGroup(resource.ApiVersion), UsageApiGroup, StringComparison.Ordinal)
            || !string.Equals(resource.Kind, UsageKind, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryReadReferences(resource, out var by, out var of)
            || !context.TryGetByName(GetApiGroup(by.ApiVersion), by.Kind, resource.Namespace(), by.Name, out var source)
            || !context.TryGetByName(GetApiGroup(of.ApiVersion), of.Kind, resource.Namespace(), of.Name, out var target)
            || source == null
            || target == null)
        {
            return;
        }

        context.Add(relationships, source, target, ResourceRelationshipKind.Reference, "uses");
    }

    private static bool TryReadReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        out ResourceReference by,
        out ResourceReference of)
    {
        if (resource is GenericKubernetesObject generic)
        {
            if (!generic.Properties.TryGetValue("spec", out var spec))
            {
                by = default;
                of = default;
                return false;
            }

            var byValid = TryReadReference(spec, "by", out by);
            var ofValid = TryReadReference(spec, "of", out of);
            return byValid && ofValid;
        }

        by = default;
        of = default;
        return false;
    }

    private static bool TryReadReference(
        JsonElement? spec,
        string propertyName,
        out ResourceReference reference)
    {
        reference = default;
        var value = spec is { } specValue
            ? RelationshipProviderHelpers.Property(specValue, propertyName)
            : null;
        if (value is not { } referenceValue)
        {
            return false;
        }

        var apiVersion = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(referenceValue, "apiVersion"));
        var kind = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(referenceValue, "kind"));
        var resourceRef = RelationshipProviderHelpers.Property(referenceValue, "resourceRef");
        var name = resourceRef is { } resourceRefValue
            ? RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(resourceRefValue, "name"))
            : null;
        if (string.IsNullOrWhiteSpace(apiVersion)
            || string.IsNullOrWhiteSpace(kind)
            || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        reference = new ResourceReference(apiVersion, kind, name);
        return true;
    }

    private static string GetApiGroup(string? apiVersion)
    {
        if (string.IsNullOrWhiteSpace(apiVersion))
        {
            return string.Empty;
        }

        var separator = apiVersion.IndexOf('/');
        return separator < 0 ? string.Empty : apiVersion[..separator];
    }

    private readonly record struct ResourceReference(string ApiVersion, string Kind, string Name);
}
