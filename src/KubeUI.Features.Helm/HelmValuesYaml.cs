using System.Text.Json;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Features.Helm;

internal static class HelmValuesYaml
{
    public static string Serialize(JsonElement values, IKubernetesYamlSerializer serializer)
    {
        if (values.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Helm release values must be a JSON object.");
        }

        return serializer.Serialize(values);
    }

    public static JsonElement Parse(string yaml, IKubernetesYamlSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        if (string.IsNullOrWhiteSpace(yaml))
        {
            using var emptyObject = JsonDocument.Parse("{}");
            return emptyObject.RootElement.Clone();
        }

        var values = serializer.Deserialize<JsonElement>(yaml, strict: true);
        if (values.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException(HelmLanguageManager.Instance.HelmReleaseEditorView_ValuesMustBeObject.CurrentValue()!);
        }

        if (values.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The YAML value must be a mapping.");
        }

        return values.Clone();
    }
}
