using System.Reflection;
using System.Xml;
using k8s.Models;

namespace KubeUI.Models.Generator;

public interface IGenerator
{
    (Assembly?, XmlDocument?) GenerateAssembly(V1CustomResourceDefinition crd, string @namespace = "KubeUI.Models");
}
