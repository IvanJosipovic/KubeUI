using System.Text.Json;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

internal static class RelationshipProviderHelpers
{
    public static JsonElement? Property(GenericKubernetesObject resource, string name)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return resource.Properties.TryGetValue(name, out var value) ? value : null;
    }

    public static JsonElement? Property(JsonElement source, string name)
    {
        if (source.ValueKind != JsonValueKind.Object || !source.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value;
    }

    public static string? String(JsonElement? value)
    {
        if (value is not { } element || element.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return element.GetString();
    }

    public static IEnumerable<JsonElement> Objects(JsonElement? value)
    {
        if (value is not { ValueKind: JsonValueKind.Array } array)
        {
            yield break;
        }

        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                yield return item;
            }
        }
    }

    public static string? ValueText(JsonElement? value)
    {
        if (value is not { } element || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : element.GetRawText();
    }

    public static V1PodSpec? PodSpec(IKubernetesObject<V1ObjectMeta> resource)
        => resource switch
        {
            V1Deployment x => x.Spec?.Template?.Spec,
            V1ReplicaSet x => x.Spec?.Template?.Spec,
            V1StatefulSet x => x.Spec?.Template?.Spec,
            V1DaemonSet x => x.Spec?.Template?.Spec,
            V1Job x => x.Spec?.Template?.Spec,
            V1CronJob x => x.Spec?.JobTemplate?.Spec?.Template?.Spec,
            V1Pod x => x.Spec,
            _ => null,
        };

    public static void AddByName(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        string apiVersion,
        string kind,
        string? namespaceName,
        string? name,
        ResourceRelationshipKind relationshipKind,
        string? label = null)
    {
        if (context.TryGet(apiVersion, kind, namespaceName, name, out var target)
            && target != null)
        {
            context.Add(relationships, source, target, relationshipKind, label);
        }
    }

    public static void AddBySelector(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        string apiGroup,
        string kind,
        V1LabelSelector? selector,
        string? namespaceName,
        ResourceRelationshipKind relationshipKind)
    {
        foreach (var target in context.SelectByLabelSelector(apiGroup, kind, selector, namespaceName))
        {
            context.Add(relationships, source, target, relationshipKind);
        }
    }
}
