using System.Reflection;
using System.Runtime.CompilerServices;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class CrossplaneUsageRelationshipProvider : IResourceRelationshipProvider
{
    private const string UsageApiGroup = "protection.crossplane.io";
    private const string UsageKind = "Usage";
    private static readonly ConditionalWeakTable<Type, UsageAccessors> AccessorsByType = new();

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

        var accessors = GetAccessors(resource.GetType());
        var spec = accessors?.Spec.GetValue(resource);
        if (accessors == null
            || spec == null
            || !TryReadReference(accessors.By, spec, out var by)
            || !TryReadReference(accessors.Of, spec, out var of)
            || !context.TryGetByName(GetApiGroup(by.ApiVersion), by.Kind, resource.Namespace(), by.Name, out var source)
            || !context.TryGetByName(GetApiGroup(of.ApiVersion), of.Kind, resource.Namespace(), of.Name, out var target)
            || source == null
            || target == null)
        {
            return;
        }

        context.Add(relationships, source, target, ResourceRelationshipKind.Reference, "uses");
    }

    private static UsageAccessors? GetAccessors(Type resourceType)
    {
        if (AccessorsByType.TryGetValue(resourceType, out var accessors))
        {
            return accessors;
        }

        var created = UsageAccessors.Create(resourceType);
        if (created != null)
        {
            AccessorsByType.Add(resourceType, created);
        }

        return created;
    }

    private static bool TryReadReference(
        ReferenceAccessors accessors,
        object spec,
        out ResourceReference reference)
    {
        reference = default;
        var value = accessors.Reference.GetValue(spec);
        if (value == null)
        {
            return false;
        }

        var resourceRef = accessors.ResourceRef.GetValue(value);
        var apiVersion = accessors.ApiVersion.GetValue(value) as string;
        var kind = accessors.Kind.GetValue(value) as string;
        var name = resourceRef == null ? null : accessors.Name.GetValue(resourceRef) as string;
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

    private sealed class UsageAccessors
    {
        public required PropertyInfo Spec { get; init; }
        public required ReferenceAccessors By { get; init; }
        public required ReferenceAccessors Of { get; init; }

        public static UsageAccessors? Create(Type resourceType)
        {
            var spec = resourceType.GetProperty("Spec", BindingFlags.Instance | BindingFlags.Public);
            if (spec == null)
            {
                return null;
            }

            var by = ReferenceAccessors.Create(spec.PropertyType, "By");
            var of = ReferenceAccessors.Create(spec.PropertyType, "Of");
            return by == null || of == null ? null : new UsageAccessors { Spec = spec, By = by, Of = of };
        }
    }

    private sealed class ReferenceAccessors
    {
        public required PropertyInfo Reference { get; init; }
        public required PropertyInfo ApiVersion { get; init; }
        public required PropertyInfo Kind { get; init; }
        public required PropertyInfo ResourceRef { get; init; }
        public required PropertyInfo Name { get; init; }

        public static ReferenceAccessors? Create(Type specType, string referenceName)
        {
            var reference = specType.GetProperty(referenceName, BindingFlags.Instance | BindingFlags.Public);
            if (reference == null)
            {
                return null;
            }

            var apiVersion = reference.PropertyType.GetProperty("ApiVersion", BindingFlags.Instance | BindingFlags.Public);
            var kind = reference.PropertyType.GetProperty("Kind", BindingFlags.Instance | BindingFlags.Public);
            var resourceRef = reference.PropertyType.GetProperty("ResourceRef", BindingFlags.Instance | BindingFlags.Public);
            var name = resourceRef?.PropertyType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
            return apiVersion == null || kind == null || resourceRef == null || name == null
                ? null
                : new ReferenceAccessors { Reference = reference, ApiVersion = apiVersion, Kind = kind, ResourceRef = resourceRef, Name = name };
        }
    }
}
