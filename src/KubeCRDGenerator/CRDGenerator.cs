using k8s.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

namespace KubeCRDGenerator;

public class CRDGenerator : ICRDGenerator
{
    private ILogger<CRDGenerator> Logger { get; set; }

    public CRDGenerator(ILogger<CRDGenerator> logger)
    {
        Logger = logger;
    }

    public string GenerateCode(V1CustomResourceDefinition crd, string @namespace = "KubeCRDGenerator.Models")
    {
        var model = new DynamicType();
        model.AddUsing = true;

        var types = new List<DynamicType>();

        var version = crd.Spec.Versions.First(x => x.Served && x.Storage);

        model.Name = crd.Spec.Names.Kind;
        model.Namespace = @namespace;

        var specModelName = model.Name + "Spec";
        var statusModelName = model.Name + "Status";

        if (version.Schema.OpenAPIV3Schema.Properties.ContainsKey("status"))
        {
            model.Implements = $"IKubernetesObject<V1ObjectMeta?>, ISpec<{specModelName}>, IStatus<{statusModelName}?>";
        }
        else
        {
            model.Implements = $"IKubernetesObject<V1ObjectMeta?>, ISpec<{specModelName}?>";
        }

        model.Constant.Add($"public const string KubeApiVersion = \"{version.Name}\";");
        model.Constant.Add($"public const string KubeKind = \"{crd.Spec.Names.Kind}\";");
        model.Constant.Add($"public const string KubeGroup = \"{crd.Spec.Group}\";");
        model.Constant.Add($"public const string KubePluralName = \"{crd.Spec.Names.Plural}\";");

        model.Attributes.Add($"[KubernetesEntity(ApiVersion = \"{version.Name}\", Group = \"{crd.Spec.Group}\", Kind = \"{crd.Spec.Names.Kind}\", PluralName = \"{crd.Spec.Names.Plural}\")]");
        model.Description = version.Schema.OpenAPIV3Schema.Description;

        model.Fields.Add(new DynamicProperty("ApiVersion", "string"));

        model.Fields.Add(new DynamicProperty("Kind", "string"));

        model.Fields.Add(new DynamicProperty("Metadata", "V1ObjectMeta"));

        var spec = version.Schema.OpenAPIV3Schema.Properties["spec"];

        var specTypes = GenerateTypes(spec, specModelName);
        types.AddRange(specTypes);

        model.Fields.Add(new DynamicProperty("Spec", specModelName));

        if (version.Schema.OpenAPIV3Schema.Properties.ContainsKey("status"))
        {
            var status = version.Schema.OpenAPIV3Schema.Properties["status"];

            var statusTypes = GenerateTypes(status, statusModelName);
            types.AddRange(statusTypes);

            model.Fields.Add(new DynamicProperty("Status", statusModelName));
        }

        var str = model.ToString() + "\n" + types.Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);

        return ArrangeUsingRoslyn(str);
    }

    public (Assembly?, XDocument?) GenerateAssembly(V1CustomResourceDefinition crd, string @namespace = "KubeCRDGenerator.Models")
    {
        var code = GenerateCode(crd, @namespace);
        return GenerateAssembly(code);
    }

    private (Assembly?, XDocument?) GenerateAssembly(string code)
    {
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
        var dotNetCoreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

        var assemblyName = Path.GetRandomFileName();

        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(V1Pod).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(JsonElement).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll"))
        };

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var peStream = new MemoryStream())
        {
            using (var xmlDocumentationStream = new MemoryStream())
            {
                var result = compilation.Emit(peStream, xmlDocumentationStream: xmlDocumentationStream);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        Logger.LogError("Error creating Assembly: {0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    peStream.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(peStream.ToArray());

                    xmlDocumentationStream.Seek(0, SeekOrigin.Begin);
                    var xml = XDocument.Load(xmlDocumentationStream);

                    return (assembly, xml);
                }
            }
        }

        return (null, null);
    }

    private string ArrangeUsingRoslyn(string csCode)
    {
        var tree = CSharpSyntaxTree.ParseText(csCode);
        var root = tree.GetRoot().NormalizeWhitespace();
        return root.ToFullString();
    }

    private List<DynamicType> GenerateTypes(V1JSONSchemaProps type, string Name)
    {
        var types = new List<DynamicType>();
        var model = new DynamicType();
        types.Add(model);

        model.Name = Name;
        model.Description = type.Description;

        if (type.XKubernetesPreserveUnknownFields == true)
        {
            model.Fields.Add(new DynamicProperty("ExtensionData", $"Dictionary<string, JsonElement>", false, null, new List<string>() { "[JsonExtensionData]" }));
        }

        if (type.Properties != null)
        {
            foreach (var property in type.Properties)
            {
                var fieldName = property.Key;

                if (fieldName == "continue" || fieldName == "ref")
                {
                    fieldName = "@" + fieldName;
                }

                switch (property.Value.Type)
                {
                    case "object":
                        model.Fields.Add(new DynamicProperty(fieldName, Name + fieldName, false, property.Value.Description));
                        types.AddRange(GenerateTypes(property.Value, Name + fieldName));

                        if (property.Value.XKubernetesPreserveUnknownFields == true)
                        {
                            model.Fields.Add(new DynamicProperty("ExtensionData", $"Dictionary<string, JsonElement>", false, null, new List<string>() { "[JsonExtensionData]" }));
                        }
                        break;

                    case "array":
                        model.Fields.Add(new DynamicProperty(fieldName, $"List<{Name + fieldName}>", false, property.Value.Description));
                        types.AddRange(GenerateTypes(property.Value, Name + fieldName));
                        break;

                    case "boolean":
                        model.Fields.Add(new DynamicProperty(fieldName, "bool", false, property.Value.Description));
                        break;

                    case "integer":

                        if (property.Value.Format == "int64")
                        {
                            model.Fields.Add(new DynamicProperty(fieldName, "long", false, property.Value.Description));
                        }
                        else
                        {
                            model.Fields.Add(new DynamicProperty(fieldName, "int", false, property.Value.Description));
                        }
                        break;

                    case "string":
                        model.Fields.Add(new DynamicProperty(fieldName, "string", false, property.Value.Description));
                        break;

                    default:
                        //throw new Exception("Unhandled Type " + property.Value.Type);
                        break;
                }
            }
        }

        return types;
    }
}
