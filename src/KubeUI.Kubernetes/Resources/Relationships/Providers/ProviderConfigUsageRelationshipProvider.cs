using System.Text.Json;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class ProviderConfigUsageRelationshipProvider : IResourceRelationshipProvider
{
    private const string UsageKind = "ProviderConfigUsage";
    private const string ProviderConfigNameLabel = "crossplane.io/provider-config";
    private const string ProviderConfigKindLabel = "crossplane.io/provider-config-kind";

    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(
            new KubernetesClient.Informer.Client.GroupApiVersionKind(string.Empty, string.Empty, UsageKind, "providerconfigusages"),
            matchAnyApiGroup: true),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is not GenericKubernetesObject usage
            || !string.Equals(resource.Kind, UsageKind, StringComparison.Ordinal)
            || !TryReadResourceReference(usage, out var resourceReference)
            || !TryReadProviderConfigReference(usage, out var providerConfigReference))
        {
            return;
        }

        if (!context.TryGet(resourceReference.ApiVersion, resourceReference.Kind, resource.Namespace(), resourceReference.Name, out var source)
            || source == null
            || !TryGetProviderConfig(context, usage, providerConfigReference, out var target)
            || target == null)
        {
            return;
        }

        context.Add(relationships, source, target, ResourceRelationshipKind.Reference, "uses");
    }

    private static bool TryReadResourceReference(GenericKubernetesObject usage, out ResourceReference reference)
    {
        reference = default;
        var value = RelationshipProviderHelpers.Property(usage, "resourceRef");
        var apiVersion = RelationshipProviderHelpers.String(Property(value, "apiVersion"));
        var kind = RelationshipProviderHelpers.String(Property(value, "kind"));
        var name = RelationshipProviderHelpers.String(Property(value, "name"));
        if (string.IsNullOrWhiteSpace(apiVersion) || string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        reference = new(apiVersion, kind, name);
        return true;
    }

    private static bool TryReadProviderConfigReference(GenericKubernetesObject usage, out ProviderConfigReference reference)
    {
        reference = default;
        var labels = usage.Metadata?.Labels;
        var labelName = GetLabel(labels, ProviderConfigNameLabel);
        var labelKind = GetLabel(labels, ProviderConfigKindLabel);
        var providerConfigRef = RelationshipProviderHelpers.Property(usage, "providerConfigRef");
        var specName = RelationshipProviderHelpers.String(Property(providerConfigRef, "name"));
        var specKind = RelationshipProviderHelpers.String(Property(providerConfigRef, "kind"));

        if ((!string.IsNullOrWhiteSpace(labelName) && !string.IsNullOrWhiteSpace(specName) && !string.Equals(labelName, specName, StringComparison.Ordinal))
            || (!string.IsNullOrWhiteSpace(labelKind) && !string.IsNullOrWhiteSpace(specKind) && !string.Equals(labelKind, specKind, StringComparison.Ordinal)))
        {
            return false;
        }

        var name = labelName ?? specName;
        var kind = labelKind ?? specKind ?? (usage.Namespace() == null ? "ClusterProviderConfig" : "ProviderConfig");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(kind))
        {
            return false;
        }

        reference = new(name, kind);
        return true;
    }

    private static bool TryGetProviderConfig(ResourceRelationshipContext context, GenericKubernetesObject usage, ProviderConfigReference reference, out IKubernetesObject<V1ObjectMeta>? providerConfig)
    {
        providerConfig = null;
        var namespaceName = string.Equals(reference.Kind, "ClusterProviderConfig", StringComparison.Ordinal) ? null : usage.Namespace();
        if (context.TryGetByExactGroupAndKind(GetApiGroup(usage.ApiVersion), reference.Kind, out var exactGroupResources)
            && (providerConfig = exactGroupResources.FirstOrDefault(candidate =>
                string.Equals(candidate.Namespace(), namespaceName, StringComparison.Ordinal)
                && string.Equals(candidate.Name(), reference.Name, StringComparison.Ordinal))) != null)
        {
            return true;
        }

        return context.TryGetUniqueByName(reference.Kind, namespaceName, reference.Name, out providerConfig);
    }

    private static JsonElement? Property(JsonElement? source, string name)
        => source is { } value ? RelationshipProviderHelpers.Property(value, name) : null;

    private static string? GetLabel(IDictionary<string, string>? labels, string name)
        => labels != null && labels.TryGetValue(name, out var value) ? value : null;

    private static string GetApiGroup(string? apiVersion)
    {
        if (string.IsNullOrWhiteSpace(apiVersion)) return string.Empty;
        var separator = apiVersion.IndexOf('/');
        return separator < 0 ? string.Empty : apiVersion[..separator];
    }

    private readonly record struct ResourceReference(string ApiVersion, string Kind, string Name);
    private readonly record struct ProviderConfigReference(string Name, string Kind);
}
