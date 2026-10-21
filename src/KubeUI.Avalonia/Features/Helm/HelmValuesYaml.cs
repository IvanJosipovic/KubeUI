using System.Text.Json;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Helm;

internal static class HelmValuesYaml
{
    public static string Serialize(IReadOnlyDictionary<string, JsonElement> values)
        => KubernetesYaml.Serialize(KubernetesYaml.Deserialize<Dictionary<string, object>>(
            k8s.KubernetesJson.Serialize(values)));

    public static IReadOnlyDictionary<string, object?> Parse(string yaml)
    {
        var values = KubernetesYaml.StrictDeserializer.Deserialize<Dictionary<string, object?>>(yaml);
        return values ?? throw new InvalidOperationException(Assets.Resources.HelmReleaseEditorView_ValuesMustBeObject);
    }
}
