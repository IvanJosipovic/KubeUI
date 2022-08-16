using k8s.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Xml;

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
        var types = new List<DynamicType>();

        var version = crd.Spec.Versions.First(x => x.Served && x.Storage);

        types.AddRange(GenerateTypes(version.Schema.OpenAPIV3Schema, crd.Spec.Names.Kind, @namespace, version.Name, crd.Spec.Names.Kind, crd.Spec.Group, crd.Spec.Names.Plural));

        var str = types.Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);

        return ArrangeUsingRoslyn(str);
    }

    public async Task<(Assembly?, XmlDocument?)> GenerateAssembly(V1CustomResourceDefinition crd, string @namespace = "KubeCRDGenerator.Models")
    {
        var code = GenerateCode(crd, @namespace);
        return await GenerateAssembly(code);
    }

    private async Task<(Assembly?, XmlDocument?)> GenerateAssembly(string code)
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
                var xml = new XmlDocument();
                xml.Load(xmlDocumentationStream);

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

    private List<DynamicType> GenerateTypes(V1JSONSchemaProps schema, string name, string @namespace = "KubeCRDGenerator.Models", string? version = null, string? kind = null, string? group = null, string? plural = null)
    {
        bool isRoot = version != null && kind != null && group != null && plural != null;

        var types = new List<DynamicType>();
        var model = new DynamicType();
        types.Add(model);

        model.Name = CapitalizeFirstLetter(name);
        model.Description = schema.Description;

        if (isRoot)
        {
            // Root Model
            model.Namespace = @namespace;
            model.AddUsing = true;

            model.Constant.Add($"public const string KubeApiVersion = \"{version}\";");
            model.Constant.Add($"public const string KubeKind = \"{kind}\";");
            model.Constant.Add($"public const string KubeGroup = \"{group}\";");
            model.Constant.Add($"public const string KubePluralName = \"{plural}\";");

            model.Attributes.Add($"[KubernetesEntity(ApiVersion = \"{version}\", Group = \"{group}\", Kind = \"{kind}\", PluralName = \"{plural}\")]");


            model.Fields.Add(new DynamicProperty("ApiVersion", "string", false, "APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources", new() { "[JsonPropertyName(\"apiVersion\")]" }));
            model.Fields.Add(new DynamicProperty("Kind", "string", false, "Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds", new() { "[JsonPropertyName(\"kind\")]" }));

            model.Implements = $"IKubernetesObject<V1ObjectMeta?>";
            model.Fields.Add(new DynamicProperty("Metadata", "V1ObjectMeta", false, "ObjectMeta is metadata that all persisted resources must have, which includes all objects users must create.", new() { $"[JsonPropertyName(\"metadata\")]" }));
        }

        if (schema.XKubernetesPreserveUnknownFields == true)
        {
            model.Fields.Add(new DynamicProperty("ExtensionData", $"Dictionary<string, object>", false, null, new List<string>() { "[JsonExtensionData]" }));
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

                var combinedFieldName = name + CapitalizeFirstLetter(property.Key.Replace("$", "").Replace("@", ""));

                if (isRoot)
                {
                    // Root Model

                    if (property.Key == "apiVersion" || property.Key == "kind" || property.Key == "metadata")
                    {
                        continue;
                    }
                }

                switch (property.Value.Type)
                {
                    case "object":
                        model.Fields.Add(new DynamicProperty(fieldName, combinedFieldName, IsNullable(property), property.Value.Description, new() { attribute }));
                        types.AddRange(GenerateTypes(property.Value, combinedFieldName));

                        if (isRoot)
                        {
                            // Root Model

                            if (property.Key.Equals("status"))
                            {
                                model.Implements += $", IStatus<{combinedFieldName + (IsNullable(property) ? "?" : "")}>";
                            }
                            else if (property.Key.Equals("spec"))
                            {
                                model.Implements += $", ISpec<{combinedFieldName + (IsNullable(property) ? "?" : "")}>";
                            }
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

                            if (isRoot)
                            {
                                // Root Model

                                if (property.Key.Equals("status"))
                                {
                                    model.Implements += $", IStatus<JsonNode{(IsNullable(property) ? "?" : "")}>";
                                }
                                else if (property.Key.Equals("spec"))
                                {
                                    model.Implements += $", ISpec<JsonNode{(IsNullable(property) ? "?" : "")}>";
                                }
                            }
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
        var references = new List<MetadataReference>();

        var assebly = this.GetType().Assembly;

        var assemblies = assebly.GetManifestResourceNames().Where(x => x.StartsWith("runtime.") && x.EndsWith(".dll")).ToList();

        foreach (var item in assemblies)
        {
            using var stream = assebly.GetManifestResourceStream(item);
            var ass = MetadataReference.CreateFromStream(stream);
            references.Add(ass);
        }

        references.AddRange(Basic.Reference.Assemblies.NetStandard20.All);

        MetadataReferences = references.ToArray();
    }
}
