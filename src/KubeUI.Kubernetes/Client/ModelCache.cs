using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public sealed class ModelCache
{
    public ConcurrentDictionary<Assembly, XmlDocument> Cache { get; } = new();

    public ConcurrentDictionary<string, Type> TypeCache { get; } = new();

    public void AddToCache(Assembly assembly, XmlDocument xmlDocument)
    {
        if (Cache.Keys.Any(x => x.FullName == assembly.FullName))
        {
            return;
        }

        Cache[assembly] = xmlDocument;

        foreach (var entry in GetTypes(assembly))
        {
            TypeCache[entry.Key] = entry.Value;
        }
    }

    public Type? GetResourceType(GroupApiVersionKind type)
    {
        return GetResourceType(type.Group, type.ApiVersion, type.Kind);
    }

    public Type? GetResourceType(string group, string version, string kind)
    {
        if (TypeCache.TryGetValue($"{group}/{version}/{kind}", out var value))
        {
            return value;
        }

        return null;
    }

    public Dictionary<string, Type> GetTypes(Assembly assembly)
    {
        var types = new Dictionary<string, Type>();

        foreach (var item in assembly.GetExportedTypes())
        {
            var attributes = item.GetCustomAttributes(typeof(KubernetesEntityAttribute), inherit: true);
            if (attributes.Length == 0)
            {
                continue;
            }

            var attribute = (KubernetesEntityAttribute)attributes[0];
            types[$"{attribute.Group}/{attribute.ApiVersion}/{attribute.Kind}"] = item;
        }

        return types;
    }

    public bool CheckIfCRDExists(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage).Name;
        return TypeCache.ContainsKey($"{crd.Spec.Group}/{version}/{crd.Spec.Names.Kind}");
    }
}

