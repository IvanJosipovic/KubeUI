using k8s.Models;
using System.Reflection;
using System.Xml.Linq;

namespace KubeCRDGenerator;

public interface ICRDGenerator
{
    /// <summary>
    /// Generates an assembly from the given CRD with the containing types
    /// </summary>
    /// <param name="crd"></param>
    /// <param name="namespace"></param>
    /// <returns>Assembly and XML Documentation</returns>
    (Assembly?, XDocument?) GenerateAssembly(V1CustomResourceDefinition crd, string @namespace = "KubeCRDGenerator.Models");

    /// <summary>
    /// Generates source code from the given CRD with the containing types
    /// </summary>
    /// <param name="crd"></param>
    /// <param name="namespace"></param>
    /// <returns></returns>
    string GenerateCode(V1CustomResourceDefinition crd, string @namespace = "KubeCRDGenerator.Models");
}
