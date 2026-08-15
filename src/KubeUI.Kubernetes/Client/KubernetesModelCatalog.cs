using System.Collections.Frozen;
using System.Reflection;
using System.Xml;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;

namespace KubeUI.Kubernetes;

public sealed class KubernetesModelCatalog
{
    private readonly FrozenDictionary<GroupApiVersionKind, Type> _types;
    private readonly FrozenDictionary<string, Type> _yamlTypeMap;
    private readonly FrozenDictionary<string, XmlElement> _documentation;

    public KubernetesModelCatalog()
    {
        var xmlDocumentation = new XmlDocument();
        using var stream = typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml")
            ?? throw new InvalidOperationException("Kubernetes XML documentation resource not found.");
        xmlDocumentation.Load(stream);

        _types = GetTypes(typeof(V1Deployment).Assembly).ToFrozenDictionary();
        _yamlTypeMap = _types.ToFrozenDictionary(
            pair => CreateYamlKey(pair.Key),
            pair => pair.Value,
            StringComparer.Ordinal);
        _documentation = GetDocumentationIndex(xmlDocumentation).ToFrozenDictionary(StringComparer.Ordinal);
    }

    public Type? GetResourceType(GroupApiVersionKind key)
    {
        _types.TryGetValue(key, out var type);
        return type;
    }

    public Type? GetResourceType(string group, string version, string kind)
    {
        return GetResourceType(CreateKey(group, version, kind));
    }

    /// <summary>
    /// Gets YAML resource keys for the built-in Kubernetes model types.
    /// </summary>
    /// <returns>A map from YAML resource keys to model types.</returns>
    public FrozenDictionary<string, Type> GetYamlTypeMap()
    {
        return _yamlTypeMap;
    }

    public XmlElement? GetDocumentation(MemberInfo memberInfo)
    {
        return _documentation.TryGetValue(
            CreateMemberDocumentationKey(memberInfo.DeclaringType, GetMemberPrefix(memberInfo), memberInfo.Name),
            out var documentation)
            ? documentation
            : null;
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

        return _documentation.TryGetValue(
            CreateMemberDocumentationKey(methodInfo.DeclaringType, 'M', memberName),
            out var documentation)
            ? documentation
            : null;
    }

    public XmlElement? GetDocumentation(Type type)
    {
        return _documentation.TryGetValue(
            CreateMemberDocumentationKey(type, 'T', string.Empty),
            out var documentation)
            ? documentation
            : null;
    }

    private static Dictionary<GroupApiVersionKind, Type> GetTypes(Assembly assembly)
    {
        var types = new Dictionary<GroupApiVersionKind, Type>();

        foreach (var item in assembly.GetExportedTypes())
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

        foreach (var member in members.ChildNodes.OfType<XmlElement>())
        {
            var name = member.GetAttribute("name");
            if (!string.IsNullOrEmpty(name))
            {
                documentation[name] = member;
            }
        }

        return documentation;
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

    private static string CreateMemberDocumentationKey(Type? type, char prefix, string name)
    {
        var typeName = type == null ? string.Empty : (type.FullName ?? type.Name).Replace('+', '.');
        return string.IsNullOrEmpty(name)
            ? $"{prefix}:{typeName}"
            : $"{prefix}:{typeName}.{name}";
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
}
