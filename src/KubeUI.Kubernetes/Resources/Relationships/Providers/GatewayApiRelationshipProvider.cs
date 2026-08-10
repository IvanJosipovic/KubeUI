using System.Reflection;
using System.Runtime.CompilerServices;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class GatewayApiRelationshipProvider : IResourceRelationshipProvider
{
    private const string ApiGroup = "gateway.networking.k8s.io";
    private const string CoreApiGroup = "";
    private static readonly ConditionalWeakTable<Type, GatewayAccessors> AccessorsByType = new();

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
        if (!string.Equals(GetApiGroup(resource.ApiVersion), ApiGroup, StringComparison.Ordinal)
            || !TryGetSpec(resource, out var accessors, out var spec))
        {
            return;
        }

        switch (resource.Kind)
        {
            case "Gateway":
                AddGatewayClassReference(resource, accessors, spec, context, relationships);
                break;
            case "HTTPRoute" or "GRPCRoute" or "TCPRoute" or "TLSRoute" or "UDPRoute":
                AddParentReferences(resource, accessors, spec, context, relationships);
                AddBackendReferences(resource, accessors, spec, context, relationships);
                break;
            case "ListenerSet":
                AddParentReference(resource, GetProperty(accessors.ParentRef, spec), context, relationships);
                break;
            case "BackendTLSPolicy":
                AddPolicyTargetReferences(resource, accessors, spec, context, relationships);
                break;
        }
    }

    private static void AddGatewayClassReference(
        IKubernetesObject<V1ObjectMeta> resource,
        GatewayAccessors accessors,
        object spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        var gatewayClassName = GetString(accessors.GatewayClassName, spec);
        AddReference(context, relationships, resource, ApiGroup, "GatewayClass", null, gatewayClassName);
    }

    private static void AddParentReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        GatewayAccessors accessors,
        object spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var parent in GetObjects(accessors.ParentRefs, spec))
        {
            AddParentReference(resource, parent, context, relationships);
        }
    }

    private static void AddParentReference(
        IKubernetesObject<V1ObjectMeta> resource,
        object? parent,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        if (parent == null)
        {
            return;
        }

        var group = GetString(parent, "Group") ?? ApiGroup;
        var kind = GetString(parent, "Kind") ?? "Gateway";
        var namespaceName = GetString(parent, "Namespace") ?? resource.Namespace();
        var name = GetString(parent, "Name");
        var label = GetReferenceLabel(parent, "SectionName", "Port");
        AddReference(context, relationships, resource, group, kind, namespaceName, name, label);
    }

    private static void AddBackendReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        GatewayAccessors accessors,
        object spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var rule in GetObjects(accessors.Rules, spec))
        {
            foreach (var backend in GetObjects(GetProperty(rule, "BackendRefs")))
            {
                AddBackendReference(context, relationships, resource, backend);
            }

            foreach (var match in GetObjects(GetProperty(rule, "Matches")))
            {
                foreach (var backend in GetObjects(GetProperty(match, "BackendRefs")))
                {
                    AddBackendReference(context, relationships, resource, backend);
                }
            }
        }
    }

    private static void AddBackendReference(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> resource,
        object backend)
    {
        var reference = GetProperty(backend, "BackendRef") ?? backend;
        var group = GetString(reference, "Group") ?? CoreApiGroup;
        var kind = GetString(reference, "Kind") ?? "Service";
        var namespaceName = GetString(reference, "Namespace") ?? resource.Namespace();
        var name = GetString(reference, "Name");
        var label = GetReferenceLabel(reference, "SectionName", "Port");
        AddReference(context, relationships, resource, group, kind, namespaceName, name, label);
    }

    private static void AddPolicyTargetReferences(
        IKubernetesObject<V1ObjectMeta> resource,
        GatewayAccessors accessors,
        object spec,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var targetReference in GetObjects(accessors.TargetRefs, spec))
        {
            var group = GetString(targetReference, "Group") ?? CoreApiGroup;
            var kind = GetString(targetReference, "Kind") ?? "Service";
            var namespaceName = GetString(targetReference, "Namespace") ?? resource.Namespace();
            var name = GetString(targetReference, "Name");
            var label = GetReferenceLabel(targetReference, "SectionName", "Port");
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

    private static bool TryGetSpec(
        IKubernetesObject<V1ObjectMeta> resource,
        out GatewayAccessors accessors,
        out object spec)
    {
        accessors = GetAccessors(resource.GetType());
        if (accessors.Spec == null || !TryGetValue(accessors.Spec, resource, out spec!))
        {
            spec = null!;
            return false;
        }

        return spec != null;
    }

    private static GatewayAccessors GetAccessors(Type resourceType)
    {
        if (AccessorsByType.TryGetValue(resourceType, out var accessors))
        {
            return accessors;
        }

        var created = GatewayAccessors.Create(resourceType);
        AccessorsByType.Add(resourceType, created);
        return created;
    }

    private static IEnumerable<object> GetObjects(PropertyInfo? property, object source)
        => GetObjects(GetProperty(property, source));

    private static IEnumerable<object> GetObjects(object? value)
    {
        if (value is not IEnumerable enumerable || value is string)
        {
            yield break;
        }

        foreach (var item in enumerable)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }

    private static object? GetProperty(object source, string propertyName)
        => GetProperty(FindProperty(source.GetType(), propertyName), source);

    private static object? GetProperty(PropertyInfo? property, object source)
    {
        if (property == null)
        {
            return null;
        }

        return TryGetValue(property, source, out var value) ? value : null;
    }

    private static bool TryGetValue(PropertyInfo property, object source, out object? value)
    {
        try
        {
            value = property.GetValue(source);
            return true;
        }
        catch (Exception exception) when (exception is TargetInvocationException or ArgumentException or InvalidOperationException)
        {
            value = null;
            return false;
        }
    }

    private static string? GetString(PropertyInfo? property, object source)
        => GetString(GetProperty(property, source));

    private static string? GetString(object source, string propertyName)
        => GetString(GetProperty(source, propertyName));

    private static string? GetString(object? value)
        => value as string;

    private static string? GetReferenceLabel(object source, params string[] properties)
    {
        List<string> values = [];
        foreach (var property in properties)
        {
            var value = GetProperty(FindProperty(source.GetType(), property), source);
            if (value != null)
            {
                values.Add($"{property[..1].ToLowerInvariant()}{property[1..]}={value}");
            }
        }

        return values.Count == 0 ? null : string.Join(", ", values);
    }

    private sealed class GatewayAccessors
    {
        public PropertyInfo? Spec { get; private init; }
        public PropertyInfo? GatewayClassName { get; private init; }
        public PropertyInfo? ParentRefs { get; private init; }
        public PropertyInfo? ParentRef { get; private init; }
        public PropertyInfo? Rules { get; private init; }
        public PropertyInfo? TargetRefs { get; private init; }

        public static GatewayAccessors Create(Type resourceType)
        {
            var spec = FindProperty(resourceType, "Spec");
            return new GatewayAccessors
            {
                Spec = spec,
                GatewayClassName = GetProperty(spec?.PropertyType, "GatewayClassName"),
                ParentRefs = GetProperty(spec?.PropertyType, "ParentRefs"),
                ParentRef = GetProperty(spec?.PropertyType, "ParentRef"),
                Rules = GetProperty(spec?.PropertyType, "Rules"),
                TargetRefs = GetProperty(spec?.PropertyType, "TargetRefs"),
            };
        }

        private static PropertyInfo? GetProperty(Type? type, string name)
            => type == null ? null : FindProperty(type, name);
    }

    private static PropertyInfo? FindProperty(Type type, string name)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => string.Equals(property.Name, name, StringComparison.Ordinal));
        return properties.OrderByDescending(property => GetTypeDepth(property.DeclaringType)).FirstOrDefault();
    }

    private static int GetTypeDepth(Type? type)
    {
        var depth = 0;
        while (type != null)
        {
            depth++;
            type = type.BaseType;
        }

        return depth;
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
