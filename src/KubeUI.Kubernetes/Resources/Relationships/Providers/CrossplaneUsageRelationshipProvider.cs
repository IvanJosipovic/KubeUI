using System.Reflection;
using System.Runtime.CompilerServices;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class CrossplaneUsageRelationshipProvider : IResourceRelationshipProvider
{
    private const string UsageApiVersion = "protection.crossplane.io/v1beta1";
    private const string UsageKind = "Usage";
    private static readonly ConditionalWeakTable<Type, UsageAccessors> AccessorsByType = new();

    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(new GroupApiVersionKind("protection.crossplane.io", "v1beta1", UsageKind, "usages")),
    ];

    public void AddRelationships(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        if (!string.Equals(resource.ApiVersion, UsageApiVersion, StringComparison.Ordinal)
            || !string.Equals(resource.Kind, UsageKind, StringComparison.Ordinal))
        {
            return;
        }

        UsageAccessors? accessors = GetAccessors(resource.GetType());
        object? spec = accessors?.Spec.GetValue(resource);
        if (accessors == null
            || spec == null
            || !TryReadReference(accessors.By, spec, out ResourceReference by)
            || !TryReadReference(accessors.Of, spec, out ResourceReference of)
            || !context.TryGet(by.ApiVersion, by.Kind, resource.Namespace(), by.Name, out IKubernetesObject<V1ObjectMeta>? source)
            || !context.TryGet(of.ApiVersion, of.Kind, resource.Namespace(), of.Name, out IKubernetesObject<V1ObjectMeta>? target)
            || source == null
            || target == null)
        {
            return;
        }

        context.Add(relationships, source, target, ResourceRelationshipKind.Reference, "uses");
    }

    private static UsageAccessors? GetAccessors(Type resourceType)
    {
        if (AccessorsByType.TryGetValue(resourceType, out UsageAccessors? accessors))
        {
            return accessors;
        }

        UsageAccessors? created = UsageAccessors.Create(resourceType);
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
        object? value = accessors.Reference.GetValue(spec);
        if (value == null)
        {
            return false;
        }

        object? resourceRef = accessors.ResourceRef.GetValue(value);
        string? apiVersion = accessors.ApiVersion.GetValue(value) as string;
        string? kind = accessors.Kind.GetValue(value) as string;
        string? name = resourceRef == null ? null : accessors.Name.GetValue(resourceRef) as string;
        if (string.IsNullOrWhiteSpace(apiVersion)
            || string.IsNullOrWhiteSpace(kind)
            || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        reference = new ResourceReference(apiVersion, kind, name);
        return true;
    }

    private readonly record struct ResourceReference(string ApiVersion, string Kind, string Name);

    private sealed class UsageAccessors
    {
        public required PropertyInfo Spec { get; init; }
        public required ReferenceAccessors By { get; init; }
        public required ReferenceAccessors Of { get; init; }

        public static UsageAccessors? Create(Type resourceType)
        {
            PropertyInfo? spec = resourceType.GetProperty("Spec", BindingFlags.Instance | BindingFlags.Public);
            if (spec == null)
            {
                return null;
            }

            ReferenceAccessors? by = ReferenceAccessors.Create(spec.PropertyType, "By");
            ReferenceAccessors? of = ReferenceAccessors.Create(spec.PropertyType, "Of");
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
            PropertyInfo? reference = specType.GetProperty(referenceName, BindingFlags.Instance | BindingFlags.Public);
            if (reference == null)
            {
                return null;
            }

            PropertyInfo? apiVersion = reference.PropertyType.GetProperty("ApiVersion", BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo? kind = reference.PropertyType.GetProperty("Kind", BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo? resourceRef = reference.PropertyType.GetProperty("ResourceRef", BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo? name = resourceRef?.PropertyType.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
            return apiVersion == null || kind == null || resourceRef == null || name == null
                ? null
                : new ReferenceAccessors { Reference = reference, ApiVersion = apiVersion, Kind = kind, ResourceRef = resourceRef, Name = name };
        }
    }
}
