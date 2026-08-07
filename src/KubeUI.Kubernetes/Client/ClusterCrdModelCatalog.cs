using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Xml;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;

namespace KubeUI.Kubernetes;

public sealed class ClusterCrdModelCatalog
{
    private readonly Lock _gate = new();

    private ImmutableDictionary<GroupApiVersionKind, Type> _types =
        ImmutableDictionary<GroupApiVersionKind, Type>.Empty;
    private ImmutableDictionary<Assembly, AssemblyEntry> _assemblies =
        ImmutableDictionary<Assembly, AssemblyEntry>.Empty;
    private ImmutableDictionary<string, Assembly> _assembliesByIdentity =
        ImmutableDictionary<string, Assembly>.Empty.WithComparers(StringComparer.Ordinal);
    private Dictionary<string, Type>? _yamlTypeMap;

    public void AddToCache(Assembly assembly, XmlDocument xmlDocument, GeneratedAssemblyUnloadHandle? unloadHandle = null)
    {
        lock (_gate)
        {
            if (_assembliesByIdentity.ContainsKey(GetAssemblyIdentity(assembly)))
            {
                unloadHandle?.Dispose();
                return;
            }

            AddAssemblyUnsafe(assembly, xmlDocument, unloadHandle);
        }
    }

    public (Type? previousType, Type? currentType) ReplaceCustomResourceDefinition(
        V1CustomResourceDefinition crd,
        Assembly assembly,
        XmlDocument xmlDocument,
        GeneratedAssemblyUnloadHandle? unloadHandle = null)
    {
        var key = GetCustomResourceDefinitionTypeKey(crd);

        lock (_gate)
        {
            _types.TryGetValue(key, out Type? previousType);
            if (previousType != null)
            {
                RemoveAssemblyUnsafe(previousType.Assembly);
            }

            RemoveAssemblyWithSameIdentityUnsafe(assembly);
            AddAssemblyUnsafe(assembly, xmlDocument, unloadHandle);
            _types.TryGetValue(key, out Type? currentType);
            return (previousType, currentType);
        }
    }

    public Type? RemoveCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        var key = GetCustomResourceDefinitionTypeKey(crd);

        lock (_gate)
        {
            if (!_types.TryGetValue(key, out Type? existingType))
            {
                return null;
            }

            RemoveAssemblyUnsafe(existingType.Assembly);
            return existingType;
        }
    }

    public void RemoveAllCustomResourceDefinitions()
    {
        lock (_gate)
        {
            foreach (var pair in _assemblies)
            {
                if (pair.Value.UnloadHandle != null)
                {
                    RemoveAssemblyUnsafe(pair.Key);
                }
            }
        }
    }

    public Type? GetResourceType(GroupApiVersionKind key)
    {
        _types.TryGetValue(key, out Type? value);
        return value;
    }

    public Type? GetResourceType(string group, string version, string kind)
    {
        return GetResourceType(CreateKey(group, version, kind));
    }

    public IReadOnlyDictionary<string, Type> GetYamlTypeMap()
    {
        lock (_gate)
        {
            return _yamlTypeMap ??= BuildYamlTypeMap();
        }
    }

    public bool CheckIfCRDExists(V1CustomResourceDefinition crd)
    {
        return GetResourceType(GetCustomResourceDefinitionTypeKey(crd)) != null;
    }

    public XmlElement? GetDocumentation(MemberInfo memberInfo)
    {
        return GetDocumentation(
            memberInfo.DeclaringType,
            GetMemberPrefix(memberInfo),
            memberInfo.Name);
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

    private void AddAssemblyUnsafe(Assembly assembly, XmlDocument xmlDocument, GeneratedAssemblyUnloadHandle? unloadHandle)
    {
        var types = GetTypes(assembly);
        var entry = new AssemblyEntry(
            GetDocumentationIndex(xmlDocument).ToFrozenDictionary(StringComparer.Ordinal),
            types.Keys.ToArray(),
            unloadHandle);

        var typeBuilder = _types.ToBuilder();
        foreach (var pair in types)
        {
            typeBuilder[pair.Key] = pair.Value;
        }

        _types = typeBuilder.ToImmutable();
        _assemblies = _assemblies.SetItem(assembly, entry);
        _assembliesByIdentity = _assembliesByIdentity.SetItem(GetAssemblyIdentity(assembly), assembly);
        _yamlTypeMap = null;
    }

    private void RemoveAssemblyWithSameIdentityUnsafe(Assembly assembly)
    {
        if (_assembliesByIdentity.TryGetValue(GetAssemblyIdentity(assembly), out Assembly? existingAssembly))
        {
            RemoveAssemblyUnsafe(existingAssembly);
        }
    }

    private void RemoveAssemblyUnsafe(Assembly assembly)
    {
        if (!_assemblies.TryGetValue(assembly, out AssemblyEntry? entry))
        {
            return;
        }

        var typeBuilder = _types.ToBuilder();
        foreach (var key in entry.TypeKeys)
        {
            typeBuilder.Remove(key);
        }

        _types = typeBuilder.ToImmutable();
        _assemblies = _assemblies.Remove(assembly);
        _assembliesByIdentity = _assembliesByIdentity.Remove(GetAssemblyIdentity(assembly));
        _yamlTypeMap = null;
        entry.UnloadHandle?.Dispose();
    }

    private Dictionary<string, Type> BuildYamlTypeMap()
    {
        var map = new Dictionary<string, Type>(_types.Count, StringComparer.Ordinal);
        foreach (var pair in _types)
        {
            map[CreateYamlKey(pair.Key)] = pair.Value;
        }

        return map;
    }

    private XmlElement? GetDocumentation(Type? type, char prefix, string name)
    {
        if (type == null
            || !_assemblies.TryGetValue(type.Assembly, out AssemblyEntry? entry))
        {
            return null;
        }

        return entry.Documentation.TryGetValue(
            CreateMemberDocumentationKey(type, prefix, name),
            out XmlElement? documentation)
            ? documentation
            : null;
    }

    private static Dictionary<GroupApiVersionKind, Type> GetTypes(Assembly assembly)
    {
        var types = new Dictionary<GroupApiVersionKind, Type>();

        foreach (Type item in assembly.GetExportedTypes())
        {
            var attributes = item.GetCustomAttributes(typeof(KubernetesEntityAttribute), inherit: true);
            if (attributes.Length == 0)
            {
                continue;
            }

            var attribute = (KubernetesEntityAttribute)attributes[0];
            types[CreateKey(attribute.Group, attribute.ApiVersion, attribute.Kind)] = item;
        }

        return types;
    }

    private static Dictionary<string, XmlElement> GetDocumentationIndex(XmlDocument xmlDocumentation)
    {
        var documentation = new Dictionary<string, XmlElement>(StringComparer.Ordinal);
        var members = xmlDocumentation["doc"]?["members"];
        if (members == null)
        {
            return documentation;
        }

        foreach (XmlElement member in members.ChildNodes.OfType<XmlElement>())
        {
            var name = member.GetAttribute("name");
            if (!string.IsNullOrEmpty(name))
            {
                documentation[name] = member;
            }
        }

        return documentation;
    }

    private static GroupApiVersionKind GetCustomResourceDefinitionTypeKey(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage).Name;
        return CreateKey(crd.Spec.Group, version, crd.Spec.Names.Kind);
    }

    private static GroupApiVersionKind CreateKey(string group, string version, string kind)
    {
        return new GroupApiVersionKind(group, version, kind, string.Empty);
    }

    private static string CreateYamlKey(GroupApiVersionKind key)
    {
        var groupPrefix = string.IsNullOrEmpty(key.Group) ? string.Empty : $"{key.Group}/";
        return $"{groupPrefix}{key.ApiVersion}/{key.Kind}";
    }

    private static string CreateMemberDocumentationKey(Type type, char prefix, string name)
    {
        var typeName = (type.FullName ?? type.Name).Replace('+', '.');
        return string.IsNullOrEmpty(name)
            ? $"{prefix}:{typeName}"
            : $"{prefix}:{typeName}.{name}";
    }

    private static string GetAssemblyIdentity(Assembly assembly)
    {
        return assembly.FullName ?? assembly.GetName().Name ?? assembly.ToString();
    }

    private static char GetMemberPrefix(MemberInfo memberInfo)
    {
        return memberInfo.MemberType switch
        {
            MemberTypes.Constructor => 'M',
            MemberTypes.Event => 'E',
            MemberTypes.Field => 'F',
            MemberTypes.Method => 'M',
            MemberTypes.NestedType => 'T',
            MemberTypes.Property => 'P',
            MemberTypes.TypeInfo => 'T',
            MemberTypes.Custom => 'M',
            _ => 'M',
        };
    }

    private sealed record AssemblyEntry(
        FrozenDictionary<string, XmlElement> Documentation,
        GroupApiVersionKind[] TypeKeys,
        GeneratedAssemblyUnloadHandle? UnloadHandle);
}
