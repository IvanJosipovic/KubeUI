using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public sealed class ModelCache
{
    private readonly Lock _gate = new();

    public ConcurrentDictionary<Assembly, XmlDocument> Cache { get; } = new();

    public ConcurrentDictionary<string, Type> TypeCache { get; } = new();

    public void AddToCache(Assembly assembly, XmlDocument xmlDocument)
    {
        lock (_gate)
        {
            if (Cache.Keys.Any(x => x.FullName == assembly.FullName))
            {
                return;
            }

            AddAssemblyUnsafe(assembly, xmlDocument);
        }
    }

    public (Type? previousType, Type? currentType) ReplaceCustomResourceDefinition(V1CustomResourceDefinition crd, Assembly assembly, XmlDocument xmlDocument)
    {
        var key = GetCustomResourceDefinitionTypeKey(crd);

        lock (_gate)
        {
            TypeCache.TryGetValue(key, out var previousType);
            if (previousType != null)
            {
                RemoveAssemblyUnsafe(previousType.Assembly);
            }

            RemoveAssembliesWithSameIdentityUnsafe(assembly);
            AddAssemblyUnsafe(assembly, xmlDocument);
            TypeCache.TryGetValue(key, out var currentType);
            return (previousType, currentType);
        }
    }

    public Type? RemoveCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        var key = GetCustomResourceDefinitionTypeKey(crd);

        lock (_gate)
        {
            if (!TypeCache.TryGetValue(key, out var existingType))
            {
                return null;
            }

            RemoveAssemblyUnsafe(existingType.Assembly);
            return existingType;
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
        return TypeCache.ContainsKey(GetCustomResourceDefinitionTypeKey(crd));
    }

    public XmlElement? GetDocumentation(MemberInfo memberInfo)
    {
        return GetDocumentation(memberInfo.DeclaringType, memberInfo.MemberType.ToString()[0], memberInfo.Name);
    }

    public XmlElement? GetDocumentation(MethodInfo methodInfo)
    {
        var parameterTypeNames = methodInfo.GetParameters()
            .Select(parameter => parameter.ParameterType.FullName)
            .Where(fullName => !string.IsNullOrWhiteSpace(fullName));

        var suffix = string.Join(",", parameterTypeNames);
        var memberName = string.IsNullOrEmpty(suffix)
            ? methodInfo.Name
            : $"{methodInfo.Name}({suffix})";

        return GetDocumentation(methodInfo.DeclaringType, 'M', memberName);
    }

    public XmlElement? GetDocumentation(Type type)
    {
        return GetDocumentation(type, 'T', string.Empty);
    }

    private void AddAssemblyUnsafe(Assembly assembly, XmlDocument xmlDocument)
    {
        Cache[assembly] = xmlDocument;

        foreach (var entry in GetTypes(assembly))
        {
            TypeCache[entry.Key] = entry.Value;
        }
    }

    private void RemoveAssembliesWithSameIdentityUnsafe(Assembly assembly)
    {
        var matches = Cache.Keys
            .Where(x => string.Equals(x.FullName, assembly.FullName, StringComparison.Ordinal))
            .ToList();

        for (var i = 0; i < matches.Count; i++)
        {
            RemoveAssemblyUnsafe(matches[i]);
        }
    }

    private void RemoveAssemblyUnsafe(Assembly assembly)
    {
        Cache.TryRemove(assembly, out _);

        var keysToRemove = TypeCache
            .Where(x => x.Value.Assembly == assembly)
            .Select(x => x.Key)
            .ToList();

        for (var i = 0; i < keysToRemove.Count; i++)
        {
            TypeCache.TryRemove(keysToRemove[i], out _);
        }
    }

    private static string GetCustomResourceDefinitionTypeKey(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage).Name;
        return $"{crd.Spec.Group}/{version}/{crd.Spec.Names.Kind}";
    }

    private XmlElement? GetDocumentation(Type? type, char prefix, string name)
    {
        if (type == null || !Cache.TryGetValue(type.Assembly, out var xmlDocument))
        {
            return null;
        }

        var typeDocumentationName = GetXmlDocumentationTypeName(type);
        var fullName = string.IsNullOrEmpty(name)
            ? $"{prefix}:{typeDocumentationName}"
            : $"{prefix}:{typeDocumentationName}.{name}";

        return xmlDocument["doc"]?["members"]?.SelectSingleNode($"member[@name='{fullName}']") as XmlElement;
    }

    private static string GetXmlDocumentationTypeName(Type type)
    {
        return (type.FullName ?? type.Name).Replace('+', '.');
    }
}

