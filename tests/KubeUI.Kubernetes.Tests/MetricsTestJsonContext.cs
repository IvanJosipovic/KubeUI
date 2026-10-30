using System.Text.Json.Serialization;

namespace KubeUI.Kubernetes.Tests;

[JsonSerializable(typeof(ClusterMetricsSettings))]
internal partial class MetricsTestJsonContext : JsonSerializerContext;
