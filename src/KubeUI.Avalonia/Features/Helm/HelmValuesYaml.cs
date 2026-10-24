using System.Text.Json;
using KubeUI.Kubernetes.Serialization;
using YamlDotNet.RepresentationModel;

namespace KubeUI.Avalonia.Features.Helm;

internal static class HelmValuesYaml
{
    public static string Serialize(JsonElement values)
    {
        if (values.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Helm release values must be a JSON object.");
        }

        return KubernetesYaml.Serialize(KubernetesYaml.Deserialize<Dictionary<string, object>>(
            k8s.KubernetesJson.Serialize(values)));
    }

    public static IReadOnlyDictionary<string, object?> Parse(string yaml)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return new Dictionary<string, object?>();
        }

        // Load the document tree first so duplicate explicit keys are rejected on the raw structure. The
        // merging parser resolves `<<` keys before the deserializer's duplicate check runs, which would
        // report legitimate explicit overrides of merged keys as duplicates.
        var stream = new YamlStream();
        using (var reader = new StringReader(yaml))
        {
            stream.Load(reader);
        }

        var values = KubernetesYaml.Deserialize<Dictionary<string, object?>>(yaml);
        return values ?? throw new InvalidOperationException(Assets.Resources.HelmReleaseEditorView_ValuesMustBeObject);
    }
}
