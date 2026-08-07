using System.Reflection;
using System.Xml;
using KubernetesCRDModelGen;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public sealed class ClusterModelCatalog
{
    private readonly KubernetesModelCatalog _sharedCatalog;

    public ClusterCrdModelCatalog CrdModels { get; }

    public KubernetesModelCatalog SharedCatalog => _sharedCatalog;

    public ClusterModelCatalog(KubernetesModelCatalog sharedCatalog)
    {
        _sharedCatalog = sharedCatalog;
        CrdModels = new ClusterCrdModelCatalog();
    }

    public Type? GetResourceType(GroupApiVersionKind type)
    {
        return CrdModels.GetResourceType(type)
            ?? _sharedCatalog.GetResourceType(type);
    }

    public Type? GetResourceType(string group, string version, string kind)
    {
        return GetResourceType(new GroupApiVersionKind(group, version, kind, string.Empty));
    }

    public IReadOnlyDictionary<string, Type> GetYamlTypeMap()
    {
        return CrdModels.GetYamlTypeMap();
    }

    public (Type? previousType, Type? currentType) ReplaceCustomResourceDefinition(
        V1CustomResourceDefinition crd,
        Assembly assembly,
        XmlDocument xmlDocument,
        GeneratedAssemblyUnloadHandle? unloadHandle = null)
    {
        return CrdModels.ReplaceCustomResourceDefinition(crd, assembly, xmlDocument, unloadHandle);
    }

    public Type? RemoveCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        return CrdModels.RemoveCustomResourceDefinition(crd);
    }

    public void RemoveAllCustomResourceDefinitions()
    {
        CrdModels.RemoveAllCustomResourceDefinitions();
    }

    public bool CheckIfCRDExists(V1CustomResourceDefinition crd)
    {
        return CrdModels.CheckIfCRDExists(crd);
    }

    public XmlElement? GetDocumentation(MemberInfo memberInfo)
    {
        return CrdModels.GetDocumentation(memberInfo) ?? _sharedCatalog.GetDocumentation(memberInfo);
    }

    public XmlElement? GetDocumentation(MethodInfo methodInfo)
    {
        return CrdModels.GetDocumentation(methodInfo) ?? _sharedCatalog.GetDocumentation(methodInfo);
    }

    public XmlElement? GetDocumentation(Type type)
    {
        return CrdModels.GetDocumentation(type) ?? _sharedCatalog.GetDocumentation(type);
    }
}
