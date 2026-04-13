using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using AvaloniaEdit.Document;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal static class YamlSchemaContext
{
    private const int IndentationSize = 2;

    public static YamlContextResult Resolve(TextDocument document, int offset, Type rootType, ModelCache modelCache)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(rootType);
        ArgumentNullException.ThrowIfNull(modelCache);

        if (document.LineCount == 0)
        {
            return YamlContextResult.Empty(rootType);
        }

        var safeOffset = Math.Clamp(offset, 0, document.TextLength);
        var location = document.GetLocation(safeOffset);
        var line = document.GetLineByNumber(location.Line);
        var lineText = document.GetText(line);
        var lineOffset = line.Offset;
        var columnOffset = Math.Clamp(safeOffset - lineOffset, 0, line.Length);
        var currentIndent = CountIndent(lineText);
        var isSequenceEntryLine = IsSequenceEntry(lineText);

        List<YamlFrame> frames = [new YamlFrame(-1, rootType)];
        for (var lineNumber = 1; lineNumber < location.Line; lineNumber++)
        {
            var previousLine = document.GetLineByNumber(lineNumber);
            ProcessLine(frames, document.GetText(previousLine));
        }

        while (frames.Count > 1 && ShouldPopScope(currentIndent, frames[^1].Indent, isSequenceEntryLine))
        {
            frames.RemoveAt(frames.Count - 1);
        }

        var frame = frames[^1];
        var containerType = ResolveCurrentLineContainerType(document, location.Line, lineText, frame);
        if (!TryParseLineKey(lineText, columnOffset, out var keyInfo))
        {
            if (!TryCreateImplicitKeyContext(document, location.Line, lineText, frame, containerType, modelCache, out var implicitContext))
            {
                return new YamlContextResult(containerType, null, null, currentIndent, currentIndent, string.Empty, []);
            }

            return implicitContext;
        }

        var member = FindYamlProperty(containerType, keyInfo.Key);
        var usedKeys = GetUsedKeysForScope(document, location.Line, keyInfo.KeyStartColumn, frame.UsedKeys);
        usedKeys.Remove(keyInfo.Key);
        var suggestions = GetCompletionItems(containerType, modelCache, usedKeys);
        return new YamlContextResult(
            containerType,
            member,
            BuildDocumentation(member, containerType, modelCache),
            keyInfo.KeyStartColumn,
            keyInfo.KeyEndColumn,
            keyInfo.KeyPrefix,
            suggestions);
    }

    public static bool TryCreateSequenceEntryInsertion(TextDocument document, int offset, Type rootType, ModelCache modelCache, out string insertionText)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(rootType);
        ArgumentNullException.ThrowIfNull(modelCache);

        insertionText = string.Empty;
        if (document.LineCount == 0)
        {
            return false;
        }

        var safeOffset = Math.Clamp(offset, 0, document.TextLength);
        var location = document.GetLocation(safeOffset);
        var line = document.GetLineByNumber(location.Line);
        var lineText = document.GetText(line);
        var lineOffset = line.Offset;
        var columnOffset = Math.Clamp(safeOffset - lineOffset, 0, line.Length);
        if (columnOffset != line.Length)
        {
            return false;
        }

        var currentIndent = CountIndent(lineText);
        var isSequenceEntryLine = IsSequenceEntry(lineText);
        List<YamlFrame> frames = [new YamlFrame(-1, rootType)];
        for (var lineNumber = 1; lineNumber < location.Line; lineNumber++)
        {
            var previousLine = document.GetLineByNumber(lineNumber);
            ProcessLine(frames, document.GetText(previousLine));
        }

        while (frames.Count > 1 && ShouldPopScope(currentIndent, frames[^1].Indent, isSequenceEntryLine))
        {
            frames.RemoveAt(frames.Count - 1);
        }

        var containerType = frames[^1].ContainerType;
        if (!TryGetLineKeyInfo(lineText, out var keyInfo)
            || !TryExtractKeyForLineContext(lineText, out var key, out var valuePart))
        {
            return false;
        }

        if (isSequenceEntryLine)
        {
            containerType = GetSequenceItemType(containerType);
        }

        var member = FindYamlProperty(containerType, key);
        if (member == null
            || !IsSequenceType(member.PropertyType)
            || !ShouldOpenChildScope(valuePart))
        {
            return false;
        }

        var childIndent = keyInfo.KeyStartColumn + IndentationSize;
        insertionText = "\n" + new string(' ', childIndent) + "- ";
        return true;
    }

    private static void ProcessLine(List<YamlFrame> frames, string lineText)
    {
        var indent = CountIndent(lineText);
        var trimmed = lineText.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
        {
            return;
        }

        var isSequenceEntryLine = IsSequenceEntry(trimmed);
        while (frames.Count > 1 && ShouldPopScope(indent, frames[^1].Indent, isSequenceEntryLine))
        {
            frames.RemoveAt(frames.Count - 1);
        }

        var frame = frames[^1];
        var containerType = frame.ContainerType;
        if (trimmed.StartsWith("- ", StringComparison.Ordinal))
        {
            containerType = GetSequenceItemType(containerType);
            trimmed = trimmed[2..].TrimStart();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                return;
            }
        }

        if (!TryExtractKey(trimmed, out var key, out var valuePart))
        {
            return;
        }

        frame.UsedKeys.Add(key);

        var member = FindYamlProperty(containerType, key);
        var childContainer = GetNestedContainerType(member?.PropertyType);
        if (childContainer == null || !ShouldOpenChildScope(valuePart))
        {
            return;
        }

        frames.Add(new YamlFrame(GetChildScopeIndent(indent, isSequenceEntryLine, member?.PropertyType), childContainer));
    }

    private static bool TryParseLineKey(string lineText, int columnOffset, out YamlLineKeyInfo keyInfo)
    {
        if (!TryGetLineKeyInfo(lineText, out keyInfo) || columnOffset < keyInfo.KeyStartColumn)
        {
            return false;
        }

        if (columnOffset > keyInfo.KeyEndColumn)
        {
            return false;
        }

        var prefixEnd = Math.Clamp(columnOffset, keyInfo.KeyStartColumn, keyInfo.KeyEndColumn);
        keyInfo = keyInfo with { KeyPrefix = lineText[keyInfo.KeyStartColumn..prefixEnd].TrimEnd() };
        return true;
    }

    private static bool TryGetLineKeyInfo(string lineText, out YamlLineKeyInfo keyInfo)
    {
        keyInfo = default;
        if (string.IsNullOrWhiteSpace(lineText))
        {
            return false;
        }

        var indent = CountIndent(lineText);
        var start = indent;
        if (lineText.Length >= start + 2 && string.CompareOrdinal(lineText, start, "- ", 0, 2) == 0)
        {
            start += 2;
            while (start < lineText.Length && lineText[start] == ' ')
            {
                start++;
            }
        }

        var colonIndex = lineText.IndexOf(':', start);
        var keyEnd = colonIndex >= 0 ? colonIndex : lineText.Length;
        if (keyEnd <= start)
        {
            return false;
        }

        var rawKey = lineText[start..keyEnd].TrimEnd();
        if (string.IsNullOrWhiteSpace(rawKey) || rawKey.StartsWith('#') || rawKey == "-")
        {
            return false;
        }

        var rawKeyEnd = start + rawKey.Length;
        keyInfo = new YamlLineKeyInfo(rawKey, start, rawKeyEnd, string.Empty);
        return true;
    }

    private static bool TryCreateImplicitKeyContext(
        TextDocument document,
        int currentLineNumber,
        string lineText,
        YamlFrame frame,
        Type containerType,
        ModelCache modelCache,
        out YamlContextResult context)
    {
        context = default!;

        if (!IsImplicitCompletionContext(lineText))
        {
            return false;
        }
        var keyStartColumn = GetImplicitKeyStartColumn(lineText);
        var usedKeys = GetUsedKeysForScope(document, currentLineNumber, keyStartColumn, frame.UsedKeys);
        context = new YamlContextResult(
            containerType,
            null,
            BuildDocumentation(null, containerType, modelCache),
            keyStartColumn,
            keyStartColumn,
            string.Empty,
            GetCompletionItems(containerType, modelCache, usedKeys));

        return true;
    }

    private static Type ResolveCurrentLineContainerType(TextDocument document, int currentLineNumber, string lineText, YamlFrame frame)
    {
        if (!IsSequenceEntry(lineText))
        {
            return frame.ContainerType;
        }

        var currentIndent = CountIndent(lineText);
        for (var lineNumber = currentLineNumber - 1; lineNumber >= 1; lineNumber--)
        {
            var previousLine = document.GetLineByNumber(lineNumber);
            var previousLineText = document.GetText(previousLine);
            var previousTrimmed = previousLineText.Trim();
            if (string.IsNullOrWhiteSpace(previousTrimmed) || previousTrimmed.StartsWith('#'))
            {
                continue;
            }

            var previousIndent = CountIndent(previousLineText);
            if (previousIndent >= currentIndent)
            {
                continue;
            }

            if (!TryExtractKeyForLineContext(previousLineText, out var key, out _))
            {
                continue;
            }

            var parentContainerType = frame.ContainerType;
            if (IsSequenceEntry(previousLineText))
            {
                parentContainerType = GetSequenceItemType(parentContainerType);
            }

            var member = FindYamlProperty(parentContainerType, key);
            return member == null
                ? frame.ContainerType
                : GetSequenceItemType(member.PropertyType);
        }

        return GetSequenceItemType(frame.ContainerType);
    }

    private static HashSet<string> GetUsedKeysForScope(
        TextDocument document,
        int currentLineNumber,
        int keyStartColumn,
        IReadOnlySet<string> usedKeysAbove)
    {
        var usedKeys = new HashSet<string>(usedKeysAbove, StringComparer.Ordinal);
        foreach (var key in CollectUsedKeysBelow(document, currentLineNumber + 1, keyStartColumn))
        {
            usedKeys.Add(key);
        }

        return usedKeys;
    }

    private static IEnumerable<string> CollectUsedKeysBelow(TextDocument document, int startLineNumber, int keyStartColumn)
    {
        for (var lineNumber = startLineNumber; lineNumber <= document.LineCount; lineNumber++)
        {
            var line = document.GetLineByNumber(lineNumber);
            var lineText = document.GetText(line);
            var trimmed = lineText.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            var indent = CountIndent(lineText);
            if (indent < keyStartColumn)
            {
                yield break;
            }

            if (TryGetLineKeyInfo(lineText, out var keyInfo) && keyInfo.KeyStartColumn == keyStartColumn)
            {
                yield return keyInfo.Key;
            }
        }
    }

    private static bool IsImplicitCompletionContext(string lineText)
    {
        if (string.IsNullOrWhiteSpace(lineText))
        {
            return true;
        }

        var trimmed = lineText.Trim();
        return trimmed is "#" or "-";
    }

    private static int GetImplicitKeyStartColumn(string lineText)
    {
        var start = CountIndent(lineText);
        if (lineText.Length >= start + 1 && lineText[start] == '-')
        {
            start++;
            while (start < lineText.Length && lineText[start] == ' ')
            {
                start++;
            }
        }

        return start;
    }

    private static bool ShouldPopScope(int currentIndent, int scopeIndent, bool isSequenceEntryLine)
    {
        return currentIndent < scopeIndent || (!isSequenceEntryLine && currentIndent == scopeIndent);
    }

    private static bool IsSequenceEntry(string lineText)
    {
        var trimmed = lineText.TrimStart();
        return trimmed == "-" || trimmed.StartsWith("- ", StringComparison.Ordinal);
    }

    private static bool TryExtractKey(string trimmedLine, out string key, out string valuePart)
    {
        key = string.Empty;
        valuePart = string.Empty;

        var colonIndex = trimmedLine.IndexOf(':');
        if (colonIndex <= 0)
        {
            return false;
        }

        key = trimmedLine[..colonIndex].Trim();
        valuePart = trimmedLine[(colonIndex + 1)..];
        return !string.IsNullOrWhiteSpace(key);
    }

    private static bool TryExtractKeyForLineContext(string lineText, out string key, out string valuePart)
    {
        var trimmed = lineText.Trim();
        if (trimmed.StartsWith("- ", StringComparison.Ordinal))
        {
            trimmed = trimmed[2..].TrimStart();
        }

        return TryExtractKey(trimmed, out key, out valuePart);
    }

    private static bool ShouldOpenChildScope(string valuePart)
    {
        if (string.IsNullOrWhiteSpace(valuePart))
        {
            return true;
        }

        var span = valuePart.AsSpan().TrimStart();
        return span.Length > 0 && span[0] == '#';
    }

    private static int GetChildScopeIndent(int lineIndent, bool isSequenceEntryLine, Type? propertyType)
    {
        _ = propertyType;
        return isSequenceEntryLine
            ? lineIndent + IndentationSize
            : lineIndent;
    }

    private static IReadOnlyList<YamlCompletionItemInfo> GetCompletionItems(
        Type containerType,
        ModelCache modelCache,
        IReadOnlySet<string>? usedKeys = null)
    {
        var normalizedType = NormalizeType(containerType);
        if (!CanEnumerateProperties(normalizedType))
        {
            return [];
        }

        return normalizedType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => CreateCompletionItem(property, modelCache))
            .Where(item => item != null)
            .Where(item => item != null && (usedKeys == null || !usedKeys.Contains(item.Text)))
            .OrderBy(item => item!.Text, StringComparer.Ordinal)
            .Cast<YamlCompletionItemInfo>()
            .ToArray();
    }

    private static YamlCompletionItemInfo? CreateCompletionItem(PropertyInfo property, ModelCache modelCache)
    {
        var yamlName = GetYamlPropertyName(property);
        if (string.IsNullOrWhiteSpace(yamlName))
        {
            return null;
        }

        var propertyType = NormalizeType(property.PropertyType);
        var requiresNestedBlock = RequiresNestedBlock(propertyType);
        return new YamlCompletionItemInfo(
            yamlName,
            requiresNestedBlock ? $"{yamlName}:" : $"{yamlName}: ",
            BuildDocumentation(property, property.DeclaringType ?? propertyType, modelCache),
            propertyType);
    }

    private static PropertyInfo? FindYamlProperty(Type containerType, string key)
    {
        var normalizedType = NormalizeType(containerType);
        if (!CanEnumerateProperties(normalizedType))
        {
            return null;
        }

        foreach (var property in normalizedType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (string.Equals(GetYamlPropertyName(property), key, StringComparison.Ordinal))
            {
                return property;
            }
        }

        return null;
    }

    private static YamlDocumentationInfo? BuildDocumentation(MemberInfo? member, Type fallbackType, ModelCache modelCache)
    {
        if (member is PropertyInfo property)
        {
            var propertyType = NormalizeType(property.PropertyType);
            var propertySummary = ExtractDocumentationText(modelCache.GetDocumentation(property));
            return new YamlDocumentationInfo(
                GetYamlPropertyName(property),
                propertyType,
                propertySummary,
                string.Empty,
                FormatDocumentationText(GetYamlPropertyName(property), propertyType, propertySummary, string.Empty));
        }

        var typeSummaryFallback = ExtractDocumentationText(modelCache.GetDocumentation(fallbackType));
        if (string.IsNullOrWhiteSpace(typeSummaryFallback))
        {
            return null;
        }

        return new YamlDocumentationInfo(
            fallbackType.Name,
            fallbackType,
            string.Empty,
            typeSummaryFallback,
            FormatDocumentationText(fallbackType.Name, fallbackType, string.Empty, typeSummaryFallback));
    }

    private static string FormatDocumentationText(string label, Type type, string propertySummary, string typeSummary)
    {
        StringBuilder builder = new();
        builder.AppendLine(label);
        builder.AppendLine(FormatTypeDisplayName(type));

        if (!string.IsNullOrWhiteSpace(propertySummary))
        {
            builder.AppendLine();
            builder.AppendLine(propertySummary);
        }

        if (!string.IsNullOrWhiteSpace(typeSummary)
            && !DocumentationTextsEqual(propertySummary, typeSummary))
        {
            builder.AppendLine();
            builder.AppendLine(typeSummary);
        }

        return builder.ToString().Trim();
    }

    private static bool DocumentationTextsEqual(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return string.Equals(
            NormalizeDocumentationText(left),
            NormalizeDocumentationText(right),
            StringComparison.Ordinal);
    }

    private static string NormalizeDocumentationText(string value)
    {
        return string.Join(
            ' ',
            value.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string FormatTypeDisplayName(Type type)
    {
        var normalizedType = NormalizeType(type);
        if (normalizedType.IsArray)
        {
            return $"{FormatTypeDisplayName(normalizedType.GetElementType() ?? typeof(object))}[]";
        }

        if (!normalizedType.IsGenericType)
        {
            return normalizedType.FullName ?? normalizedType.Name;
        }

        var genericTypeDefinition = normalizedType.GetGenericTypeDefinition();
        var genericTypeName = genericTypeDefinition.FullName ?? genericTypeDefinition.Name;
        var tickIndex = genericTypeName.IndexOf('`');
        if (tickIndex >= 0)
        {
            genericTypeName = genericTypeName[..tickIndex];
        }

        var genericArguments = normalizedType.GetGenericArguments()
            .Select(FormatTypeDisplayName);

        return $"{genericTypeName}<{string.Join(", ", genericArguments)}>";
    }

    private static string ExtractDocumentationText(XmlElement? documentation)
    {
        if (documentation == null)
        {
            return string.Empty;
        }

        List<string> parts = [];
        AppendDocumentation(parts, documentation.SelectSingleNode("summary")?.InnerText);
        AppendDocumentation(parts, documentation.SelectSingleNode("remarks")?.InnerText);
        return string.Join(Environment.NewLine + Environment.NewLine, parts);
    }

    private static void AppendDocumentation(List<string> parts, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add(NormalizeDocumentationText(value));
        }
    }

    private static string GetYamlPropertyName(PropertyInfo property)
    {
        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }

    private static Type NormalizeType(Type type)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        return nullableType ?? type;
    }

    private static Type GetSequenceItemType(Type type)
    {
        var normalizedType = NormalizeType(type);
        if (normalizedType == typeof(string))
        {
            return normalizedType;
        }

        if (normalizedType.IsArray)
        {
            return NormalizeType(normalizedType.GetElementType() ?? typeof(object));
        }

        if (normalizedType.IsGenericType)
        {
            var genericTypeDefinition = normalizedType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(List<>)
                || genericTypeDefinition == typeof(IList<>)
                || genericTypeDefinition == typeof(ICollection<>)
                || genericTypeDefinition == typeof(IEnumerable<>)
                || genericTypeDefinition == typeof(IReadOnlyList<>)
                || genericTypeDefinition == typeof(IReadOnlyCollection<>))
            {
                return NormalizeType(normalizedType.GetGenericArguments()[0]);
            }
        }

        var enumerableInterface = normalizedType.GetInterfaces()
            .FirstOrDefault(interfaceType => interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface == null
            ? normalizedType
            : NormalizeType(enumerableInterface.GetGenericArguments()[0]);
    }

    private static Type? GetNestedContainerType(Type? propertyType)
    {
        if (propertyType == null)
        {
            return null;
        }

        var normalizedType = NormalizeType(propertyType);
        if (normalizedType == typeof(string)
            || normalizedType.IsPrimitive
            || normalizedType.IsEnum
            || normalizedType == typeof(decimal)
            || normalizedType == typeof(DateTime)
            || normalizedType == typeof(DateTimeOffset)
            || normalizedType == typeof(Guid))
        {
            return null;
        }

        if (typeof(System.Collections.IDictionary).IsAssignableFrom(normalizedType))
        {
            return null;
        }

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(normalizedType) && normalizedType != typeof(string))
        {
            var itemType = GetSequenceItemType(normalizedType);
            return CanEnumerateProperties(itemType) ? itemType : null;
        }

        return normalizedType;
    }

    private static bool IsSequenceType(Type type)
    {
        var normalizedType = NormalizeType(type);
        return normalizedType != typeof(string)
            && typeof(System.Collections.IEnumerable).IsAssignableFrom(normalizedType)
            && !typeof(System.Collections.IDictionary).IsAssignableFrom(normalizedType);
    }

    private static bool RequiresNestedBlock(Type type)
    {
        return GetNestedContainerType(type) != null;
    }

    private static bool CanEnumerateProperties(Type type)
    {
        return type != typeof(string)
            && !type.IsPrimitive
            && !type.IsEnum
            && type != typeof(decimal)
            && type != typeof(DateTime)
            && type != typeof(DateTimeOffset)
            && type != typeof(Guid)
            && !typeof(System.Collections.IDictionary).IsAssignableFrom(type);
    }

    private static int CountIndent(string line)
    {
        var count = 0;
        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }

        return count;
    }

    private sealed class YamlFrame(int indent, Type containerType)
    {
        public int Indent { get; } = indent;

        public Type ContainerType { get; } = containerType;

        public HashSet<string> UsedKeys { get; } = new(StringComparer.Ordinal);
    }

    private readonly record struct YamlLineKeyInfo(string Key, int KeyStartColumn, int KeyEndColumn, string KeyPrefix);
}

internal sealed record YamlContextResult(
    Type ContainerType,
    PropertyInfo? CurrentProperty,
    YamlDocumentationInfo? Documentation,
    int KeyStartColumn,
    int KeyEndColumn,
    string KeyPrefix,
    IReadOnlyList<YamlCompletionItemInfo> CompletionItems)
{
    public static YamlContextResult Empty(Type rootType)
    {
        return new YamlContextResult(rootType, null, null, 0, 0, string.Empty, []);
    }
}

internal sealed record YamlDocumentationInfo(
    string Label,
    Type Type,
    string PropertySummary,
    string TypeSummary,
    string DisplayText);

internal sealed record YamlCompletionItemInfo(
    string Text,
    string InsertionText,
    YamlDocumentationInfo? Documentation,
    Type Type);
