using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Swordfish.NET.Collections.Auxiliary;

namespace KubeUI.Client;

[ServiceDescriptor<ModelCache>(ServiceLifetime.Transient)]
public sealed class ModelCache
{
    public readonly ConcurrentDictionary<Assembly, XmlDocument> Cache = new();
    public readonly ConcurrentDictionary<string, Type> TypeCache = new();

    public void AddToCache(Assembly assembly, XmlDocument xmlDocument)
    {
        if (!Cache.ContainsKey(assembly))
        {
            Cache[assembly] = xmlDocument;

            TypeCache.AddRange(GetTypes(assembly));
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
        var dic = new Dictionary<string, Type>();

        if (assembly == null)
        {
            return dic;
        }

        foreach (var item in assembly.GetExportedTypes())
        {
            var attributes = item.GetCustomAttributes(typeof(KubernetesEntityAttribute), true);

            if (attributes.Length > 0)
            {
                var attribute = (KubernetesEntityAttribute)attributes[0];

                dic[$"{attribute.Group}/{attribute.ApiVersion}/{attribute.Kind}"] = item;
            }
        }

        return dic;
    }

    public bool CheckIfCRDExists(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage).Name;

        return TypeCache.ContainsKey($"{crd.Spec.Group}/{version}/{crd.Spec.Names.Kind}");
    }
}
