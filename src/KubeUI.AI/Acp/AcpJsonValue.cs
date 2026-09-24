using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamJsonRpc.Protocol;

namespace KubeUI.AI.Acp;

internal static class AcpJsonValue
{
    public static string? Serialize(object? value)
    {
        if (value is null)
            return null;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteValue(writer, value);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static JsonElement? ParseObject(object? value)
    {
        switch (value)
        {
            case JsonElement element when element.ValueKind == JsonValueKind.Object:
                return element;
            case JsonDocument document when document.RootElement.ValueKind == JsonValueKind.Object:
                return document.RootElement;
            case JsonNode node:
                using (var parsedNode = JsonDocument.Parse(node.ToJsonString()))
                {
                    return parsedNode.RootElement.ValueKind == JsonValueKind.Object
                        ? parsedNode.RootElement.Clone()
                        : null;
                }
            case JObject jsonObject:
                using (var parsedObject = JsonDocument.Parse(jsonObject.ToString(Formatting.None)))
                {
                    return parsedObject.RootElement.Clone();
                }
            default:
                if (value is null)
                    return null;
                using (var parsedValue = JsonDocument.Parse(Serialize(value)!))
                {
                    return parsedValue.RootElement.ValueKind == JsonValueKind.Object
                        ? parsedValue.RootElement.Clone()
                        : null;
                }
        }
    }

    private static void WriteValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case JsonElement element:
                element.WriteTo(writer);
                return;
            case JsonDocument document:
                document.RootElement.WriteTo(writer);
                return;
            case JsonNode node:
                node.WriteTo(writer);
                return;
            case JToken token:
                using (var parsedToken = JsonDocument.Parse(token.ToString(Formatting.None)))
                {
                    parsedToken.RootElement.WriteTo(writer);
                }
                return;
            case CommonErrorData errorData:
                WriteCommonErrorData(writer, errorData);
                return;
            case string text:
                writer.WriteStringValue(text);
                return;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                return;
            case byte number:
                writer.WriteNumberValue(number);
                return;
            case sbyte number:
                writer.WriteNumberValue(number);
                return;
            case short number:
                writer.WriteNumberValue(number);
                return;
            case ushort number:
                writer.WriteNumberValue(number);
                return;
            case int number:
                writer.WriteNumberValue(number);
                return;
            case uint number:
                writer.WriteNumberValue(number);
                return;
            case long number:
                writer.WriteNumberValue(number);
                return;
            case ulong number:
                writer.WriteNumberValue(number);
                return;
            case float number:
                writer.WriteNumberValue(number);
                return;
            case double number:
                writer.WriteNumberValue(number);
                return;
            case decimal number:
                writer.WriteNumberValue(number);
                return;
            case IDictionary dictionary:
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key is not string key)
                        throw new NotSupportedException("ACP JSON object keys must be strings.");
                    writer.WritePropertyName(key);
                    if (entry.Value is null)
                        writer.WriteNullValue();
                    else
                        WriteValue(writer, entry.Value);
                }
                writer.WriteEndObject();
                return;
            case IEnumerable sequence:
                writer.WriteStartArray();
                foreach (var item in sequence)
                {
                    if (item is null)
                        writer.WriteNullValue();
                    else
                        WriteValue(writer, item);
                }
                writer.WriteEndArray();
                return;
            default:
                throw new NotSupportedException($"ACP JSON value type '{value.GetType().FullName}' is not supported.");
        }
    }

    private static void WriteCommonErrorData(Utf8JsonWriter writer, CommonErrorData errorData)
    {
        writer.WriteStartObject();
        if (errorData.TypeName is not null)
            writer.WriteString("type", errorData.TypeName);
        if (errorData.Message is not null)
            writer.WriteString("message", errorData.Message);
        if (errorData.StackTrace is not null)
            writer.WriteString("stack", errorData.StackTrace);
        writer.WriteNumber("code", errorData.HResult);
        if (errorData.Inner is not null)
        {
            writer.WritePropertyName("inner");
            WriteCommonErrorData(writer, errorData.Inner);
        }
        writer.WriteEndObject();
    }
}
