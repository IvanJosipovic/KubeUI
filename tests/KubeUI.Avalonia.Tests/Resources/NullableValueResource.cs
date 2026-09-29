using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Tests.Resources;

internal sealed class NullableValueResource : IKubernetesObject<V1ObjectMeta>
{
    public string ApiVersion { get; set; } = "v1";
    public string Kind { get; set; } = "Test";
    public V1ObjectMeta Metadata { get; set; } = new();
    public int? Value { get; set; }
}
