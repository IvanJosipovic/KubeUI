using System.Text.Json;
using k8s;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubernetesJsonSerializer : IKubernetesJsonSerializer
{
    public KubernetesJsonSerializer(JsonSerializerOptions options)
    {
        options.MakeReadOnly();
        Options = options;
    }

    private JsonSerializerOptions Options { get; }

    public void ConfigureClient(KubernetesClientConfiguration configuration)
    {
        configuration.AddJsonOptions(clientOptions => clientOptions.TypeInfoResolver = Options.TypeInfoResolver);
    }

    public string Serialize(object value) => KubernetesJson.Serialize(value, Options);

    public T Deserialize<T>(string json) => KubernetesJson.Deserialize<T>(json, Options);
}
