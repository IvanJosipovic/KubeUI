using AvaloniaEdit.Document;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using Microsoft.OpenApi;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal static class YamlSchemaContext
{
    private const int IndentationSize = 2;

    public static YamlContextResult Resolve(TextDocument document, int offset, GroupApiVersionKind kind, ClusterModelCatalog modelCache)
        => Resolve(document, offset, CreateRoot(kind, modelCache));

    internal static YamlContextResult Resolve(TextDocument document, int offset, YamlSchemaNode root)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.LineCount == 0)
            return YamlContextResult.Empty(root);

        var safeOffset = Math.Clamp(offset, 0, document.TextLength);
        var location = document.GetLocation(safeOffset);
        var line = document.GetLineByNumber(location.Line);
        var lineText = document.GetText(line);
        var lineOffset = line.Offset;
        var column = Math.Clamp(safeOffset - lineOffset, 0, line.Length);
        var indent = CountIndent(lineText);
        var sequenceEntry = IsSequenceEntry(lineText);
        var frames = BuildFrames(document, location.Line, root);

        var preserveBlankScope = string.IsNullOrWhiteSpace(lineText)
            && location.Line > 1
            && ShouldPreserveBlankScope(document, location.Line);
        while ((!string.IsNullOrWhiteSpace(lineText) || !preserveBlankScope)
            && frames.Count > 1
            && ShouldPopScope(indent, frames[^1].Indent, sequenceEntry))
            frames.RemoveAt(frames.Count - 1);

        var frame = frames[^1];
        var container = ResolveCurrentContainer(document, location.Line, lineText, frame);
        if (string.IsNullOrWhiteSpace(lineText) && frame.Schema.IsSequence)
            container = frame.Schema.Items ?? frame.Schema;
        if (!TryParseLineKey(lineText, column, out var key))
        {
            if (TryGetLineKeyInfo(lineText, out var valueKey)
                && container.Properties.GetValueOrDefault(valueKey.Key) is { } valueProperty
                && TryGetEnumValueContext(lineText, column, valueKey, valueProperty, lineOffset, out var enumValueContext))
            {
                return new YamlContextResult(
                    container,
                    CreateDocumentation(valueProperty),
                    enumValueContext,
                    GetEnumCompletionItems(valueProperty));
            }

            if (!IsImplicitCompletionContext(lineText))
                return new YamlContextResult(container, null, YamlKeyContext.Empty(lineOffset + indent), []);

            var start = GetImplicitKeyStartColumn(lineText);
            var used = GetUsedKeysForScope(document, location.Line, start, frame.UsedKeys);
            return new YamlContextResult(
                container,
                CreateDocumentation(container),
                YamlKeyContext.Empty(lineOffset + start),
                GetCompletionItems(container, used));
        }

        var property = container.Properties.GetValueOrDefault(key.Key);
        var usedKeys = GetUsedKeysForScope(document, location.Line, key.KeyStartColumn, frame.UsedKeys);
        usedKeys.Remove(key.Key);

        if (property is not null
            && TryGetEnumValueContext(lineText, column, key, property, lineOffset, out var valueContext))
        {
            return new YamlContextResult(
                container,
                CreateDocumentation(property),
                valueContext,
                GetEnumCompletionItems(property));
        }

        return new YamlContextResult(
            container,
            CreateDocumentation(property),
            new YamlKeyContext(lineOffset + key.KeyStartColumn, lineOffset + key.KeyEndColumn, key.Prefix),
            GetCompletionItems(container, usedKeys));
    }

    public static bool TryCreateSequenceEntryInsertion(TextDocument document, int offset, GroupApiVersionKind kind, ClusterModelCatalog modelCache, out string insertionText)
        => TryCreateSequenceEntryInsertion(document, offset, CreateRoot(kind, modelCache), out insertionText);

    internal static bool TryCreateSequenceEntryInsertion(TextDocument document, int offset, YamlSchemaNode root, out string insertionText)
    {
        insertionText = string.Empty;
        if (document.LineCount == 0)
            return false;

        var safeOffset = Math.Clamp(offset, 0, document.TextLength);
        var location = document.GetLocation(safeOffset);
        var line = document.GetLineByNumber(location.Line);
        var lineText = document.GetText(line);
        if (safeOffset - line.Offset != line.Length)
            return false;

        var indent = CountIndent(lineText);
        var frames = BuildFrames(document, location.Line, root);

        while (frames.Count > 1 && ShouldPopScope(indent, frames[^1].Indent, IsSequenceEntry(lineText)))
            frames.RemoveAt(frames.Count - 1);

        var container = frames[^1].Schema;
        if (IsSequenceEntry(lineText))
            container = container.Items ?? container;

        if (!TryExtractKeyForLineContext(lineText, out var key, out var value))
            return false;

        if (!container.Properties.TryGetValue(key, out var property)
            && (frames.Count <= 1 || !frames[^2].Schema.Properties.TryGetValue(key, out property)))
            return false;

        if (!property.IsSequence
            || !ShouldOpenChildScope(value))
            return false;

        var info = GetLineKeyInfo(lineText);
        insertionText = "\n" + new string(' ', info.KeyStartColumn + IndentationSize) + "- ";
        return true;
    }

    internal static YamlSchemaNode CreateRoot(GroupApiVersionKind kind, ClusterModelCatalog modelCache)
    {
        ArgumentNullException.ThrowIfNull(modelCache);
        return YamlSchemaNode.Create(kind.Kind, modelCache.OpenApiSchemas.GetSchema(kind), modelCache);
    }

    private static void ProcessLine(List<YamlFrame> frames, string text)
    {
        var indent = CountIndent(text);
        var trimmed = text.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            return;

        var sequenceEntry = IsSequenceEntry(trimmed);
        while (frames.Count > 1 && ShouldPopScope(indent, frames[^1].Indent, sequenceEntry))
            frames.RemoveAt(frames.Count - 1);

        var container = frames[^1].Schema;
        if (container.IsSequence && indent > frames[^1].Indent)
            container = container.Items ?? container;

        if (trimmed.StartsWith("- ", StringComparison.Ordinal))
        {
            container = container.Items ?? container;
            trimmed = trimmed[2..].TrimStart();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                return;
        }

        if (!TryExtractKey(trimmed, out var key, out var value))
            return;

        frames[^1].UsedKeys.Add(key);
        if (container.Properties.TryGetValue(key, out var property) && ShouldOpenChildScope(value))
            frames.Add(new YamlFrame(sequenceEntry ? indent + IndentationSize : indent, property));
    }

    private static List<YamlFrame> BuildFrames(TextDocument document, int lineNumber, YamlSchemaNode root)
    {
        List<YamlFrame> frames = [new(-1, root)];
        for (var currentLine = 1; currentLine < lineNumber; currentLine++)
            ProcessLine(frames, document.GetText(document.GetLineByNumber(currentLine)));

        return frames;
    }

    private static YamlSchemaNode ResolveCurrentContainer(TextDocument document, int lineNumber, string lineText, YamlFrame frame)
    {
        var currentIndent = CountIndent(lineText);
        if (!IsSequenceEntry(lineText))
        {
            if (frame.Schema.IsSequence && currentIndent > frame.Indent)
                return frame.Schema.Items ?? frame.Schema;

            return frame.Schema;
        }

        if (frame.Schema.IsSequence && currentIndent >= frame.Indent)
            return frame.Schema.Items ?? frame.Schema;

        for (var number = lineNumber - 1; number >= 1; number--)
        {
            var previous = document.GetText(document.GetLineByNumber(number));
            if (string.IsNullOrWhiteSpace(previous) || previous.TrimStart().StartsWith('#'))
                continue;
            if (CountIndent(previous) > currentIndent || !TryExtractKeyForLineContext(previous, out var key, out _))
                continue;

            var parent = frame.Schema;
            if (IsSequenceEntry(previous))
                parent = parent.Items ?? parent;
            return parent.Properties.GetValueOrDefault(key)?.Items ?? frame.Schema;
        }

        return frame.Schema.Items ?? frame.Schema;
    }

    private static IReadOnlyList<YamlCompletionItemInfo> GetCompletionItems(YamlSchemaNode container, IReadOnlySet<string> usedKeys)
        => container.Properties
            .Where(property => !usedKeys.Contains(property.Key))
            .Select(property => new YamlCompletionItemInfo(
                property.Key,
                property.Value.IsObject ? property.Key + ":" : property.Key + ": ",
                CreateDocumentation(property.Value),
                property.Value))
            .OrderBy(item => item.Text, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<YamlCompletionItemInfo> GetEnumCompletionItems(YamlSchemaNode schema)
        => schema.EnumValues
            .Select(value => new YamlCompletionItemInfo(value, value, null, schema))
            .ToArray();

    private static bool TryGetEnumValueContext(
        string lineText,
        int column,
        YamlLineKeyInfo key,
        YamlSchemaNode property,
        int lineOffset,
        out YamlKeyContext context)
    {
        context = YamlKeyContext.Empty(lineOffset + key.KeyEndColumn);
        if (property.EnumValues.Count == 0)
        {
            return false;
        }

        var colon = lineText.IndexOf(':', key.KeyEndColumn);
        if (colon < 0 || column <= colon)
        {
            return false;
        }

        var valueStart = colon + 1;
        while (valueStart < lineText.Length && char.IsWhiteSpace(lineText[valueStart]))
        {
            valueStart++;
        }

        var valueEnd = Math.Max(valueStart, lineText.Length);
        var prefixEnd = Math.Clamp(column, valueStart, valueEnd);
        context = new YamlKeyContext(
            lineOffset + valueStart,
            lineOffset + valueEnd,
            lineText[valueStart..prefixEnd]);
        return true;
    }

    private static YamlDocumentationInfo? CreateDocumentation(YamlSchemaNode? schema)
    {
        if (schema is null || (string.IsNullOrWhiteSpace(schema.Description) && string.IsNullOrWhiteSpace(schema.TypeName)))
            return null;

        var summary = NormalizeDocumentationText(schema.Description);
        return new YamlDocumentationInfo(schema.Name, schema.TypeName, summary);
    }

    private static HashSet<string> GetUsedKeysForScope(TextDocument document, int lineNumber, int keyColumn, IReadOnlySet<string> above)
    {
        var used = new HashSet<string>(above, StringComparer.Ordinal);
        for (var number = lineNumber + 1; number <= document.LineCount; number++)
        {
            var text = document.GetText(document.GetLineByNumber(number));
            if (string.IsNullOrWhiteSpace(text) || text.TrimStart().StartsWith('#'))
                continue;
            if (CountIndent(text) < keyColumn)
                break;
            if (TryGetLineKeyInfo(text, out var info) && info.KeyStartColumn == keyColumn)
                used.Add(info.Key);
        }
        return used;
    }

    private static bool TryParseLineKey(string text, int column, out YamlLineKeyInfo info)
    {
        if (!TryGetLineKeyInfo(text, out info) || column < info.KeyStartColumn || column > info.KeyEndColumn)
            return false;
        var end = Math.Clamp(column, info.KeyStartColumn, info.KeyEndColumn);
        info = info with { Prefix = text[info.KeyStartColumn..end].TrimEnd() };
        return true;
    }

    private static bool TryGetLineKeyInfo(string text, out YamlLineKeyInfo info)
    {
        info = default;
        var start = CountIndent(text);
        if (text.Length >= start + 2 && text[start..].StartsWith("- ", StringComparison.Ordinal))
        {
            start += 2;
            while (start < text.Length && text[start] == ' ')
                start++;
        }

        var colon = text.IndexOf(':', start);
        var end = colon < 0 ? text.Length : colon;
        var key = text[start..end].TrimEnd();
        if (string.IsNullOrWhiteSpace(key) || key.StartsWith('#') || key == "-")
            return false;
        info = new YamlLineKeyInfo(key, start, start + key.Length, string.Empty);
        return true;
    }

    private static YamlLineKeyInfo GetLineKeyInfo(string text)
        => TryGetLineKeyInfo(text, out var info) ? info : default;

    private static bool TryExtractKey(string text, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;
        var colon = text.IndexOf(':');
        if (colon <= 0)
            return false;
        key = text[..colon].Trim();
        value = text[(colon + 1)..];
        return !string.IsNullOrWhiteSpace(key);
    }

    private static bool TryExtractKeyForLineContext(string text, out string key, out string value)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("- ", StringComparison.Ordinal))
            trimmed = trimmed[2..].TrimStart();
        return TryExtractKey(trimmed, out key, out value);
    }

    private static bool ShouldOpenChildScope(string value)
        => string.IsNullOrWhiteSpace(value) || value.AsSpan().TrimStart().StartsWith("#", StringComparison.Ordinal);

    private static bool ShouldPreserveBlankScope(TextDocument document, int lineNumber)
    {
        var previousLine = document.GetText(document.GetLineByNumber(lineNumber - 1));
        return TryExtractKey(previousLine.Trim(), out _, out var value)
            && ShouldOpenChildScope(value);
    }

    private static bool IsImplicitCompletionContext(string text)
        => string.IsNullOrWhiteSpace(text) || text.Trim() is "#" or "-";

    private static int GetImplicitKeyStartColumn(string text)
    {
        var start = CountIndent(text);
        if (start < text.Length && text[start] == '-')
        {
            start++;
            while (start < text.Length && text[start] == ' ')
                start++;
        }
        return start;
    }

    private static bool ShouldPopScope(int current, int scope, bool sequenceEntry)
        => current < scope || (!sequenceEntry && current == scope);

    private static bool IsSequenceEntry(string text)
    {
        var trimmed = text.TrimStart();
        return trimmed == "-" || trimmed.StartsWith("- ", StringComparison.Ordinal);
    }

    private static int CountIndent(string text)
    {
        var count = 0;
        while (count < text.Length && text[count] == ' ')
            count++;
        return count;
    }

    private static string NormalizeDocumentationText(string? value)
        => string.Join(' ', (value ?? string.Empty).Split(default(string[]), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private sealed class YamlFrame(int indent, YamlSchemaNode schema)
    {
        public int Indent { get; } = indent;
        public YamlSchemaNode Schema { get; } = schema;
        public HashSet<string> UsedKeys { get; } = new(StringComparer.Ordinal);
    }

    private readonly record struct YamlLineKeyInfo(string Key, int KeyStartColumn, int KeyEndColumn, string Prefix);
}

internal sealed record YamlSchemaNode(
    string Name,
    JsonSchemaType SchemaType,
    string? Description,
    IReadOnlyDictionary<string, YamlSchemaNode> Properties,
    YamlSchemaNode? Items,
    IReadOnlyList<string> EnumValues)
{
    private static readonly Dictionary<string, IOpenApiSchema> EmptyProperties = new(StringComparer.Ordinal);

    public string TypeName => SchemaType.ToString().ToLowerInvariant();

    public bool IsObject => Properties.Count > 0 || SchemaType == JsonSchemaType.Object;
    public bool IsSequence => SchemaType == JsonSchemaType.Array || Items is not null;

    public static YamlSchemaNode Create(string name, IOpenApiSchema? schema, ClusterModelCatalog catalog)
        => Create(name, schema, catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance));

    private static YamlSchemaNode Create(
        string name,
        IOpenApiSchema? schema,
        ClusterModelCatalog catalog,
        HashSet<IOpenApiSchema> activeSchemas)
    {
        var description = schema?.Description;
        schema = catalog.OpenApiSchemas.ExpandReferences(schema);
        if (schema is null)
            return new(name, JsonSchemaType.Object, null, new Dictionary<string, YamlSchemaNode>(StringComparer.Ordinal), null, []);

        if (!activeSchemas.Add(schema))
            return new(name, schema.Type ?? JsonSchemaType.Object, description, new Dictionary<string, YamlSchemaNode>(StringComparer.Ordinal), null, []);

        try
        {
            var variants = GetSchemaVariants(schema, catalog).ToArray();
            var properties = new Dictionary<string, YamlSchemaNode>(StringComparer.Ordinal);
            foreach (var property in variants.SelectMany(variant => variant.Properties ?? EmptyProperties))
            {
                properties[property.Key] = Create(property.Key, property.Value, catalog, activeSchemas);
            }

            var itemsSchema = variants.Select(variant => variant.Items).FirstOrDefault(items => items is not null);
            var items = itemsSchema is null ? null : Create(name, itemsSchema, catalog, activeSchemas);
            var schemaType = variants.Select(variant => variant.Type).FirstOrDefault(type => type is not null);
            var variantDescription = variants.Select(variant => variant.Description).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            var enumValues = variants
                .SelectMany(variant => variant.Enum ?? [])
                .Select(value => value?.ToString() ?? string.Empty)
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return new(
                name,
                properties.Count > 0
                    ? JsonSchemaType.Object
                    : items is not null
                        ? JsonSchemaType.Array
                        : schemaType ?? JsonSchemaType.String,
                description ?? variantDescription,
                properties,
                items,
                enumValues);
        }
        finally
        {
            activeSchemas.Remove(schema);
        }
    }

    private static IEnumerable<IOpenApiSchema> GetSchemaVariants(IOpenApiSchema schema, ClusterModelCatalog catalog)
        => GetSchemaVariants(schema, catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance));

    private static IEnumerable<IOpenApiSchema> GetSchemaVariants(
        IOpenApiSchema schema,
        ClusterModelCatalog catalog,
        HashSet<IOpenApiSchema> visited)
    {
        if (!visited.Add(schema))
        {
            yield break;
        }

        yield return schema;

        foreach (var composed in schema.AllOf ?? [])
        {
            var resolved = catalog.OpenApiSchemas.ExpandReferences(composed);
            if (resolved is not null)
            {
                foreach (var variant in GetSchemaVariants(resolved, catalog, visited))
                {
                    yield return variant;
                }
            }
        }

        foreach (var composed in schema.OneOf ?? [])
        {
            var resolved = catalog.OpenApiSchemas.ExpandReferences(composed);
            if (resolved is not null)
            {
                foreach (var variant in GetSchemaVariants(resolved, catalog, visited))
                {
                    yield return variant;
                }
            }
        }

        foreach (var composed in schema.AnyOf ?? [])
        {
            var resolved = catalog.OpenApiSchemas.ExpandReferences(composed);
            if (resolved is not null)
            {
                foreach (var variant in GetSchemaVariants(resolved, catalog, visited))
                {
                    yield return variant;
                }
            }
        }
    }

}

internal sealed record YamlContextResult(
    YamlSchemaNode ContainerType,
    YamlDocumentationInfo? Documentation,
    YamlKeyContext Key,
    IReadOnlyList<YamlCompletionItemInfo> CompletionItems)
{
    public static YamlContextResult Empty(YamlSchemaNode root) => new(root, null, YamlKeyContext.Empty(0), []);
}

internal sealed record YamlKeyContext(int StartOffset, int EndOffset, string Prefix)
{
    public static YamlKeyContext Empty(int offset) => new(offset, offset, string.Empty);
}

internal sealed record YamlDocumentationInfo(string Label, string TypeName, string PropertySummary);

internal sealed record YamlCompletionItemInfo(string Text, string InsertionText, YamlDocumentationInfo? Documentation, YamlSchemaNode Schema);
