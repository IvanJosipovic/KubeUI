using System.Text.Json;

namespace KubeUI.Kubernetes.Serialization;

internal static class KubeConfigExtensionValueCodec
{
    public static JsonElement SerializeToElement(object? value)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            Write(writer, value);
        }

        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.Clone();
    }

    public static void Write(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                return;
            case JsonElement element:
                element.WriteTo(writer);
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
            case short number:
                writer.WriteNumberValue(number);
                return;
            case int number:
                writer.WriteNumberValue(number);
                return;
            case long number:
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
            case System.Collections.IDictionary dictionary:
                writer.WriteStartObject();
                foreach (System.Collections.DictionaryEntry pair in dictionary)
                {
                    writer.WritePropertyName(pair.Key?.ToString() ?? string.Empty);
                    Write(writer, pair.Value);
                }

                writer.WriteEndObject();
                return;
            case System.Collections.IEnumerable sequence:
                writer.WriteStartArray();
                foreach (var item in sequence)
                {
                    Write(writer, item);
                }

                writer.WriteEndArray();
                return;
            default:
                throw new JsonException($"Unsupported kubeconfig extension value type '{value.GetType()}'.");
        }
    }

    public static object? Read(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                Dictionary<object, object> dictionary = [];
                foreach (var property in value.EnumerateObject())
                {
                    dictionary.Add(property.Name, Read(property.Value)!);
                }

                return dictionary;
            case JsonValueKind.Array:
                List<object> sequence = [];
                foreach (var item in value.EnumerateArray())
                {
                    sequence.Add(Read(item)!);
                }

                return sequence;
            case JsonValueKind.String:
                return value.GetString();
            case JsonValueKind.Number:
                return value.TryGetInt64(out var integer) ? integer : value.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new JsonException("Unsupported kubeconfig extension data value.");
        }
    }
}
