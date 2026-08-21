using System.Text.Json;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class GatewayApiRelationshipProvider : IResourceRelationshipProvider
{
    private const string ApiGroup = "gateway.networking.k8s.io";
    private const string CoreApiGroup = "";

    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(new GroupApiVersionKind(ApiGroup, "v1", "BackendTLSPolicy", "backendtlspolicies"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "Gateway", "gateways"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "GatewayClass", "gatewayclasses"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "GRPCRoute", "grpcroutes"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "HTTPRoute", "httproutes"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "ListenerSet", "listenersets"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "ReferenceGrant", "referencegrants"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "TCPRoute", "tcproutes"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "TLSRoute", "tlsroutes"), allowServedVersionFallback: true),
        new(new GroupApiVersionKind(ApiGroup, "v1", "UDPRoute", "udproutes"), allowServedVersionFallback: true),
    ];

    public void AddRelationships(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        if (!string.Equals(GetApiGroup(resource.ApiVersion), ApiGroup, StringComparison.Ordinal))
        {
            return;
        }

        if (resource is not GenericKubernetesObject generic)
        {
            return;
        }

        var spec = RelationshipProviderHelpers.Property(generic, "spec");
        if (spec is not { ValueKind: JsonValueKind.Object } specValue)
        {
            return;
        }

        switch (resource.Kind)
        {
            case "Gateway":
                AddGatewayClassReference(resource, specValue, context, relationships);
                break;
            case "HTTPRoute" or "GRPCRoute" or "TCPRoute" or "TLSRoute" or "UDPRoute":
                AddParentReferences(resource, specValue, context, relationships);
                AddBackendReferences(resource, specValue, context, relationships);
                break;
            case "ListenerSet":
                AddParentReference(resource, RelationshipProviderHelpers.Property(specValue, "parentRef"), context, relationships);
                break;
            case "BackendTLSPolicy":
                AddPolicyTargetReferences(resource, specValue, context, relationships);
                break;
        }
    }

    private static void AddGatewayClassReference(
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        var gatewayClassName = RelationshipProviderHelpers.String(
            RelationshipProviderHelpers.Property(spec, "gatewayClassName"));
        AddReference(context, relationships, resource, ApiGroup, "GatewayClass", null, gatewayClassName);
    }

    private static void AddParentReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var parent in RelationshipProviderHelpers.Objects(
                     RelationshipProviderHelpers.Property(spec, "parentRefs")))
        {
            AddParentReference(resource, parent, context, relationships);
        }
    }

    private static void AddParentReference(
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement? parent,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        if (parent is not { } parentValue)
        {
            return;
        }

        var group = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(parentValue, "group")) ?? ApiGroup;
        var kind = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(parentValue, "kind")) ?? "Gateway";
        var namespaceName = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(parentValue, "namespace")) ?? resource.Namespace();
        var name = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(parentValue, "name"));
        var label = GetReferenceLabel(parentValue, "sectionName", "port");
        AddReference(context, relationships, resource, group, kind, namespaceName, name, label);
    }

    private static void AddBackendReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var rule in RelationshipProviderHelpers.Objects(
                     RelationshipProviderHelpers.Property(spec, "rules")))
        {
            AddBackends(resource, RelationshipProviderHelpers.Property(rule, "backendRefs"), context, relationships);
            foreach (var match in RelationshipProviderHelpers.Objects(
                         RelationshipProviderHelpers.Property(rule, "matches")))
            {
                AddBackends(resource, RelationshipProviderHelpers.Property(match, "backendRefs"), context, relationships);
            }
        }
    }

    private static void AddBackends(
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement? backends,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var backend in RelationshipProviderHelpers.Objects(backends))
        {
            AddBackendReference(context, relationships, resource, backend);
        }
    }

    private static void AddBackendReference(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement backend)
    {
        var reference = RelationshipProviderHelpers.Property(backend, "backendRef") ?? backend;
        var group = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(reference, "group")) ?? CoreApiGroup;
        var kind = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(reference, "kind")) ?? "Service";
        var namespaceName = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(reference, "namespace")) ?? resource.Namespace();
        var name = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(reference, "name"));
        var label = GetReferenceLabel(reference, "sectionName", "port");
        AddReference(context, relationships, resource, group, kind, namespaceName, name, label);
    }

    private static void AddPolicyTargetReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        JsonElement spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var targetReference in RelationshipProviderHelpers.Objects(
                     RelationshipProviderHelpers.Property(spec, "targetRefs")))
        {
            var group = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(targetReference, "group")) ?? CoreApiGroup;
            var kind = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(targetReference, "kind")) ?? "Service";
            var namespaceName = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(targetReference, "namespace")) ?? resource.Namespace();
            var name = RelationshipProviderHelpers.String(RelationshipProviderHelpers.Property(targetReference, "name"));
            var label = GetReferenceLabel(targetReference, "sectionName", "port");
            AddReference(context, relationships, resource, group, kind, namespaceName, name, label);
        }
    }

    private static void AddReference(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        string group,
        string kind,
        string? namespaceName,
        string? name,
        string? label = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (!context.TryGetByGroupAndKind(group, kind, out var candidates))
        {
            context.RecordUnresolved(group, kind, namespaceName, name);
            return;
        }

        var target = candidates.FirstOrDefault(candidate =>
            string.Equals(candidate.Namespace(), namespaceName, StringComparison.Ordinal)
            && string.Equals(candidate.Name(), name, StringComparison.Ordinal));
        if (target == null)
        {
            context.RecordUnresolved(group, kind, namespaceName, name);
            return;
        }

        context.Add(relationships, source, target, ResourceRelationshipKind.Reference, label);
    }

    private static string? GetReferenceLabel(JsonElement source, params string[] properties)
    {
        List<string> values = [];
        foreach (var property in properties)
        {
            var value = RelationshipProviderHelpers.ValueText(
                RelationshipProviderHelpers.Property(source, property));
            if (value is not null)
            {
                values.Add($"{property}={value}");
            }
        }

        return values.Count == 0 ? null : string.Join(", ", values);
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
}
