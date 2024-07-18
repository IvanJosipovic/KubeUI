using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Xml;
using k8s;
using k8s.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace KubeUI.Models.Generator;

public class Generator : IGenerator
{
    public const string ModelNamespace = "KubeUI.Models";

    private readonly MetadataReference[] _metadataReferences;

    private static readonly List<string> s_keywords =
    [
        "abstract",
        "as",
        "base",
        "bool",
        "break",
        "byte",
        "case",
        "catch",
        "char",
        "checked",
        "class",
        "const",
        "continue",
        "decimal",
        "default",
        "delegate",
        "do",
        "double",
        "else",
        "enum",
        "event",
        "explicit",
        "extern",
        "false",
        "finally",
        "fixed",
        "float",
        "for",
        "foreach",
        "goto",
        "if",
        "implicit",
        "in",
        "int",
        "interface",
        "internal",
        "is",
        "lock",
        "long",
        "namespace",
        "new",
        "null",
        "object",
        "operator",
        "out",
        "override",
        "params",
        "private",
        "protected",
        "public",
        "readonly",
        "ref",
        "return",
        "sbyte",
        "sealed",
        "short",
        "sizeof",
        "stackalloc",
        "static",
        "string",
        "struct",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "uint",
        "ulong",
        "unchecked",
        "unsafe",
        "ushort",
        "using",
        "virtual",
        "void",
        "volatile",
        "while"
    ];

    private static readonly List<char> s_propertyNameBadChars = [
        '-',
        '$',
        '`',
        '!',
        '@',
        '#',
        '%',
        '^',
        '&',
        '*',
        '(',
        ')',
        '+',
        '~',
        '_',
        '=',
        '.'
    ];

    private static readonly List<char> s_namespaceBadChars =
    [
        '-',
        '$',
        '`',
        '!',
        '@',
        '#',
        '%',
        '^',
        '&',
        '*',
        '(',
        ')',
        '+',
        '~',
        '_',
        '='
    ];

    private static readonly List<char> s_classBadChars =
    [
        '-',
        '$',
        '`',
        '!',
        '@',
        '#',
        '%',
        '^',
        '&',
        '*',
        '(',
        ')',
        '+',
        '~',
        '_',
        '='
    ];

    private const string KubePreserveUnkownFields = "x-kubernetes-preserve-unknown-fields";

    private const string KubeIntOrString = "x-kubernetes-int-or-string";

    private readonly CSharpCompilationOptions _options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        .WithConcurrentBuild(true)
        .WithDeterministic(true)
        .WithNullableContextOptions(NullableContextOptions.Enable)
        .WithOptimizationLevel(OptimizationLevel.Release)
        .WithOverflowChecks(false)
        .WithPlatform(Platform.AnyCpu)
        .WithSpecificDiagnosticOptions(new KeyValuePair<string, ReportDiagnostic>[]
        {
            // Don't warn for binding redirects
            new("CS1701", ReportDiagnostic.Suppress),
            new("CS1702", ReportDiagnostic.Suppress)
        });

    public Generator()
    {
        _metadataReferences ??= GetReferences();
    }

    public CompilationUnitSyntax GenerateCode(V1CustomResourceDefinition crd, string @namespace = ModelNamespace)
    {
        var version = crd.Spec.Versions.First(x => x.Served && x.Storage);

        var schema = version.Schema.OpenAPIV3Schema;

        var doc = new OpenApiStringReader().ReadFragment<OpenApiSchema>(KubernetesJson.Serialize(version.Schema.OpenAPIV3Schema), OpenApiSpecVersion.OpenApi3_0, out var diag);

        if (diag?.Errors.Count > 0)
        {
            Console.WriteLine("Error: " + diag.Errors.Select(x => x.Message));
        }

        var namespaceDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(GetCleanNamespace(@namespace + "." + crd.Spec.Group))).NormalizeWhitespace();

        namespaceDeclaration = namespaceDeclaration.AddMembers(GenerateClass(doc, crd.Spec.Names.Kind, version.Name, crd.Spec.Names.Kind, crd.Spec.Group, crd.Spec.Names.Plural));

        var compilationUnit = SyntaxFactory.CompilationUnit().WithUsings(GenerateUsings());

        return compilationUnit.AddMembers(namespaceDeclaration);
    }

    private static SyntaxList<UsingDirectiveSyntax> GenerateUsings()
    {
        var using1 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("k8s"));
        var using2 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("k8s.Models"));
        var using3 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json"));
        var using4 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Nodes"));
        var using5 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization"));
        var using6 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
        var using7 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"));

        return SyntaxFactory.List([using1, using2, using3, using4, using5, using6, using7]);
    }

    private ClassDeclarationSyntax[] GenerateClass(OpenApiSchema schema, string name, string? version = null, string? kind = null, string? group = null, string? plural = null)
    {
        bool isRoot = version != null && kind != null && group != null && plural != null;

        var classes = new List<ClassDeclarationSyntax>();

        var @class = SyntaxFactory.ClassDeclaration(GetCleanClassName((isRoot ? version : string.Empty) + name))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        if (isRoot)
        {
            // Base Classes
            @class = @class.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IKubernetesObject<V1ObjectMeta>")));

            // Create an attribute syntax for the KubernetesEntity attribute
            var kubernetesEntityAttribute = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("KubernetesEntity"))
                .WithArgumentList(
                    SyntaxFactory.AttributeArgumentList()
                    .AddArguments(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Group")),
                            null,
                            SyntaxFactory.IdentifierName("KubeGroup")),
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Kind")),
                            null,
                            SyntaxFactory.IdentifierName("KubeKind")),
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("ApiVersion")),
                            null,
                            SyntaxFactory.IdentifierName("KubeApiVersion")),
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("PluralName")),
                            null,
                            SyntaxFactory.IdentifierName("KubePluralName"))));

            @class = @class.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(kubernetesEntityAttribute));

            // Create the field declarations for the KubernetesEntity attribute
            var kubeApiVersion = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("string"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator("KubeApiVersion")
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(version!)))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            var kubeKind = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("string"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator("KubeKind")
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(kind!)))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            var kubeGroup = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("string"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator("KubeGroup")
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(group!)))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            var kubePluralName = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("string"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator("KubePluralName")
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(plural!)))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            // Create a property declaration for ApiVersion
            var apiVersion = CreateProperty("string", "apiVersion");

            // Create a property declaration for Kind
            var kindProp = CreateProperty("string", "kind");

            // Create a property declaration for Metadata
            var metaProp = CreateProperty("V1ObjectMeta", "metadata");

            @class = @class.AddMembers(kubeApiVersion, kubeKind, kubeGroup, kubePluralName, apiVersion, kindProp, metaProp);
        }

        if (schema.Extensions.TryGetValue(KubePreserveUnkownFields, out var preserve) && preserve is OpenApiBoolean preserveBool && preserveBool.Value)
        {
            // Create property
            var property = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.NullableType(
                        SyntaxFactory.ParseTypeName("IDictionary<string, JsonElement>")
                    ),
                    SyntaxFactory.Identifier("ExtensionData")
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                    )
                )
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List(
                            new AccessorDeclarationSyntax[]
                            {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }
                        )
                    )
                )
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.ParseName("JsonExtensionData")))
                        )
                    )
                );

            @class = @class.AddMembers(property);
        }

        foreach (var property in schema.Properties)
        {
            if (isRoot)
            {
                // Root Model, skip these fields as we are adding them above
                if (property.Key == "apiVersion" || property.Key == "kind" || property.Key == "metadata")
                {
                    continue;
                }
            }

            var type = GetOrGenerateType(property.Value, classes, @class.Identifier.Text, property.Key);

            // Add ISpec base class
            if (isRoot && property.Key == "spec")
            {
                @class = @class.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"ISpec<{type}>")));
            }

            // Add IStatus base class
            if (isRoot && property.Key == "status")
            {
                @class = @class.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IStatus<{type}>")));
            }

            @class = @class.AddMembers(CreateProperty(type, property.Key, property.Value.Description, schema.Required.Contains(property.Key)));
        }

        classes.Add(@class);

        return [.. classes];
    }

    private string GetOrGenerateType(OpenApiSchema schema, List<ClassDeclarationSyntax> classes, string parentClassName, string propertyName)
    {
        string type = string.Empty;

        if (schema.Extensions.TryGetValue(KubePreserveUnkownFields, out var value2) && value2 is OpenApiBoolean boolean2 && boolean2.Value)
        {
            type = nameof(JsonNode);
        }
        else if (schema.Extensions.TryGetValue(KubeIntOrString, out var value) && value is OpenApiBoolean boolean && boolean.Value)
        {
            type = nameof(IntstrIntOrString);
        }

        switch (schema.Type)
        {
            case "object":
                if (string.IsNullOrEmpty(type))
                {
                    if (schema.AdditionalProperties != null)
                    {
                        type = $"IDictionary<string, {GetOrGenerateType(schema.AdditionalProperties, classes, parentClassName, propertyName)}>";
                    }
                    else
                    {
                        var nestedClasses = GenerateClass(schema, parentClassName + GetCleanClassName(propertyName));

                        classes.AddRange(nestedClasses);

                        type = nestedClasses[^1].Identifier.Text;
                    }
                }

                break;
            case "string":
                type = "string";
                break;
            case "number":
                type = "double";
                break;
            case "boolean":
                type = "bool";
                break;
            case "integer":
                if (schema.Format == "int64") type = "long"; else type = "int";
                break;
            case "array":
                type = $"IList<{GetOrGenerateType(schema.Items, classes, parentClassName, propertyName)}>";
                break;
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new Exception("Unsupported Type: " + type);
        }

        return type;
    }

    private static PropertyDeclarationSyntax CreateProperty(string typeName, string propertyName, string comment = "", bool required = true)
    {
        return SyntaxFactory.PropertyDeclaration(required ? SyntaxFactory.ParseTypeName(typeName) : SyntaxFactory.NullableType(SyntaxFactory.ParseTypeName(typeName)), GetCleanPropertyName(propertyName))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                        new AccessorDeclarationSyntax[]{
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        })))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("JsonPropertyName"))
                            .WithArgumentList(
                                SyntaxFactory.AttributeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(
                                        SyntaxFactory.AttributeArgument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(propertyName)))))))))
                .WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.Comment($"/// <summary>{comment}</summary>"),
                        SyntaxFactory.CarriageReturnLineFeed));
    }

    private static string GetCleanNamespace(string name)
    {
        foreach (var badChar in s_namespaceBadChars)
        {
            if (name.Contains(badChar))
            {
                name = name.Replace(badChar.ToString(), string.Empty);
            }
        }

        foreach (var keyword in s_keywords)
        {
            if (name.Contains('.' + keyword + '.') || name.StartsWith(keyword + '.') || name.EndsWith('.' + keyword))
            {
                name = name.Replace(keyword, "@" + keyword);
            }
        }

        return name;
    }

    private static string GetCleanClassName(string name)
    {
        foreach (var badChar in s_classBadChars)
        {
            if (name.Contains(badChar))
            {
                name = name.Replace(badChar.ToString(), string.Empty);
            }
        }

        return CapitalizeFirstLetter(name);
    }

    private static string GetCleanPropertyName(string name)
    {
        foreach (var badChar in s_propertyNameBadChars)
        {
            if (name.Contains(badChar))
            {
                name = name.Replace(badChar.ToString(), string.Empty);
            }
        }

        return CapitalizeFirstLetter(name);
    }

    private static string CapitalizeFirstLetter(string str)
    {
        if (str.Length == 0)
        {
            return string.Empty;
        }
        else if (str.Length == 1)
        {
            return char.ToUpper(str[0], CultureInfo.InvariantCulture).ToString();
        }
        else
        {
            return char.ToUpper(str[0], CultureInfo.InvariantCulture) + str[1..];
        }
    }

    public (Assembly?, XmlDocument?) GenerateAssembly(V1CustomResourceDefinition crd, string @namespace = ModelNamespace)
    {
        try
        {
            var code = GenerateCode(crd, @namespace);

            var compilation = CSharpCompilation.Create(
                crd.Metadata.Name,
                syntaxTrees: new[] { code.SyntaxTree },
                references: _metadataReferences,
                options: _options);

            using var peStream = new MemoryStream();
            using var xmlDocumentationStream = new MemoryStream();

            var result = compilation.Emit(peStream, xmlDocumentationStream: xmlDocumentationStream);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (var diagnostic in failures)
                {
                    Console.WriteLine("Error creating Assembly: {0}: {1}", diagnostic.Id, diagnostic.GetMessage());
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
            Console.WriteLine("Error creating Assembly: {0}", ex);
        }

        return (null, null);
    }

    private MetadataReference[] GetReferences()
    {
        var references = new List<MetadataReference>();

        var assembly = GetType().Assembly;

        var assemblies = assembly.GetManifestResourceNames().Where(x => x.StartsWith("runtime.") && x.EndsWith(".dll")).ToList();

        foreach (var item in assemblies)
        {
            using var stream = assembly.GetManifestResourceStream(item);
            var ass = MetadataReference.CreateFromStream(stream!);
            references.Add(ass);
        }

        references.Add(Basic.Reference.Assemblies.Net80.References.System);
        references.Add(Basic.Reference.Assemblies.Net80.References.SystemTextJson);
        references.Add(Basic.Reference.Assemblies.Net80.References.SystemRuntime);

        return [.. references];
    }
}
