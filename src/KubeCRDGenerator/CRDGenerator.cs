using k8s.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace KubeCRDGenerator;

public class CRDGenerator : ICRDGenerator
{
    private ILogger<CRDGenerator> Logger { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private MetadataReference[] MetadataReferences { get; set; }

    public CRDGenerator(ILogger<CRDGenerator> logger, IHttpClientFactory httpClientFactory)
    {
        Logger = logger;
        HttpClientFactory = httpClientFactory;
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

        model.Fields.Add(new DynamicProperty("ApiVersion", "string", false, null, new() { "[JsonPropertyName(\"apiVersion\")]" }));

        model.Fields.Add(new DynamicProperty("Kind", "string", false, null, new() { "[JsonPropertyName(\"kind\")]" }));

        model.Fields.Add(new DynamicProperty("Metadata", "V1ObjectMeta", false, null, new() { "[JsonPropertyName(\"metadata\")]" }));

        var spec = version.Schema.OpenAPIV3Schema.Properties["spec"];

        var specTypes = GenerateTypes(spec, specModelName);
        types.AddRange(specTypes);

        model.Fields.Add(new DynamicProperty("Spec", specModelName, false, null, new() { "[JsonPropertyName(\"spec\")]" }));

        if (version.Schema.OpenAPIV3Schema.Properties.ContainsKey("status"))
        {
            var status = version.Schema.OpenAPIV3Schema.Properties["status"];

            var statusTypes = GenerateTypes(status, statusModelName);
            types.AddRange(statusTypes);

            model.Fields.Add(new DynamicProperty("Status", statusModelName, false, null, new() { "[JsonPropertyName(\"status\")]" }));
        }

        var str = model.ToString() + "\n" + types.Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);

        return ArrangeUsingRoslyn(str);
    }

    public async Task<(Assembly?, XDocument?)> GenerateAssembly(V1CustomResourceDefinition crd, string @namespace = "KubeCRDGenerator.Models")
    {
        var code = GenerateCode(crd, @namespace);
        return await GenerateAssembly(code);
    }

    private async Task<(Assembly?, XDocument?)> GenerateAssembly(string code)
    {
        try
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));

            var assemblyName = Path.GetRandomFileName();

            if (MetadataReferences == null)
            {
                await GenerateReferences();
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: MetadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var peStream = new MemoryStream();
            using var xmlDocumentationStream = new MemoryStream();

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
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating Assembly");
        }

        return (null, null);
    }

    private static string ArrangeUsingRoslyn(string csCode)
    {
        var tree = CSharpSyntaxTree.ParseText(csCode);
        var root = tree.GetRoot().NormalizeWhitespace();

        return root.ToFullString();
    }

    private List<DynamicType> GenerateTypes(V1JSONSchemaProps schema, string Name)
    {
        var types = new List<DynamicType>();
        var model = new DynamicType();
        types.Add(model);

        model.Name = CapitalizeFirstLetter(Name);
        model.Description = schema.Description;

        if (schema.XKubernetesPreserveUnknownFields == true)
        {
            model.Fields.Add(new DynamicProperty("ExtensionData", $"Dictionary<string, JsonNode>", false, null, new List<string>() { "[JsonExtensionData]" }));
        }

        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties)
            {
                var attribute = $"[JsonPropertyName(\"{property.Key}\")]";
                string fieldName = property.Key;

                if (fieldName == "continue" || fieldName == "ref" || fieldName == "namespace" || fieldName == "static")
                {
                    fieldName = "@" + fieldName;
                }

                if (fieldName.Contains("$"))
                {
                    fieldName = fieldName.Replace("$", "");
                }

                fieldName = CapitalizeFirstLetter(fieldName);

                var combinedFieldName = Name + CapitalizeFirstLetter(property.Key.Replace("$", "").Replace("@", ""));

                switch (property.Value.Type)
                {
                    case "object":
                        model.Fields.Add(new DynamicProperty(fieldName, combinedFieldName, IsNullable(property), property.Value.Description, new() { attribute }));
                        types.AddRange(GenerateTypes(property.Value, combinedFieldName));

                        if (property.Value.XKubernetesPreserveUnknownFields == true)
                        {
                            model.Fields.Add(new DynamicProperty("ExtensionData", $"IDictionary<string, JsonNode>", IsNullable(property), null, new List<string>() { "[JsonExtensionData]" }));
                        }
                        break;

                    case "array":
                        model.Fields.Add(new DynamicProperty(fieldName, $"IList<{combinedFieldName}>", IsNullable(property), property.Value.Description, new() { attribute }));
                        types.AddRange(GenerateTypes(property.Value, combinedFieldName));
                        break;

                    case "boolean":
                        model.Fields.Add(new DynamicProperty(fieldName, "bool", IsNullable(property), property.Value.Description, new() { attribute }));
                        break;

                    case "integer":
                        if (property.Value.Format == "int64")
                        {
                            model.Fields.Add(new DynamicProperty(fieldName, "long", IsNullable(property), property.Value.Description, new() { attribute }));
                        }
                        else
                        {
                            model.Fields.Add(new DynamicProperty(fieldName, "int", IsNullable(property), property.Value.Description, new() { attribute }));
                        }
                        break;

                    case "string":
                        model.Fields.Add(new DynamicProperty(fieldName, "string", IsNullable(property), property.Value.Description, new() { attribute }));
                        break;

                    case "":
                    case null:
                        if (property.Value.XKubernetesPreserveUnknownFields == true)
                        {
                            model.Fields.Add(new DynamicProperty(fieldName, $"JsonNode", IsNullable(property), property.Value.Description, new() { attribute }));
                        }
                        break;

                    default:
                        Logger.LogWarning("Unhandled Property Type {type}", property.Value.Type);
                        break;
                }
            }
        }
        else if (schema.Type == "array")
        {

            //model.Fields.Add(new DynamicProperty(fieldName, $"IList<{Name + fieldName}>", IsNullable(property), property.Value.Description, new() { attribute }));
            //types.AddRange(GenerateTypes(property.Value, Name + fieldName));
        }
        else if (schema.Type == "object")
        {
            //model.Fields.Add(new DynamicProperty(fieldName, $"IList<{Name + fieldName}>", IsNullable(property), property.Value.Description, new() { attribute }));
            //types.AddRange(GenerateTypes(property.Value, Name + fieldName));
        }
        else
        {
            Logger.LogWarning("Unhandled Type {type}", schema.Type);
        }

        return types;
    }

    private static bool IsNullable(KeyValuePair<string, V1JSONSchemaProps> item)
    {
        if (item.Value.Required == null)
        {
            return true;
        }

        return !item.Value.Required.Contains(item.Key);
    }

    private static string CapitalizeFirstLetter(string str)
    {
        if (str.Length == 0)
        {
            return string.Empty;
        }
        else if (str.Length == 1)
        {
            return char.ToUpper(str[0]).ToString();
        }
        else
        {
            return char.ToUpper(str[0]) + str[1..];
        }
    }

    private async Task GenerateReferences()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            var assemblies = new string[]
            {
                "netstandard.dll",
                "System.Private.CoreLib.dll",
                "System.Linq.dll",
                "KubernetesClient.Models.dll",
                "System.ComponentModel.Primitives.dll",
                "System.Private.CoreLib.dll",
                "System.Text.Json.dll",
                "System.Runtime.dll",
            };

            var references = new List<MetadataReference>(assemblies.Length);

            foreach (var assemblie in assemblies)
            {
                var data = await HttpClientFactory.CreateClient().GetAsync("_framework/" + assemblie);

                using (var task = await data.Content.ReadAsStreamAsync())
                {
                    references.Add(MetadataReference.CreateFromStream(task));
                }
            }

            MetadataReferences = references.ToArray();
        }
        else
        {
            var dotNetCoreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

            MetadataReferences = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(V1Pod).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonNode).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll"))
            };
        }
    }
}
