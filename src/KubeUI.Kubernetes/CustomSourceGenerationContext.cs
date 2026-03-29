using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using static k8s.KubernetesJson;

namespace KubeUI.Kubernetes;

[JsonSerializable(typeof(Dictionary<object, object>))]
[JsonSerializable(typeof(ICollection))]
[JsonSerializable(typeof(ICollection<KeyValuePair<object, object>>))]
[JsonSerializable(typeof(IDictionary))]
[JsonSerializable(typeof(IDictionary<object, object>))]
[JsonSerializable(typeof(IEnumerable))]
[JsonSerializable(typeof(IEnumerable<KeyValuePair<object, object>>))]
[JsonSerializable(typeof(ISerializable))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(k8s.Watcher<k8s.KubernetesObject>.WatchEvent))]
[JsonSerializable(typeof(KeyValuePair<object, object>))]
[JsonSerializable(typeof(KubernetesObject))]
[JsonSerializable(typeof(ValueType))]
[JsonSerializable(typeof(WatchEventType))]
[JsonSerializable(typeof(IReadOnlyDictionary<object, object>))]
[JsonSerializable(typeof(IReadOnlyCollection<KeyValuePair<object, object>>))]
[JsonSerializable(typeof(IDeserializationCallback))]

[JsonSerializable(typeof(KubernetesList<V1CustomResourceDefinition>))]
[JsonSerializable(typeof(k8s.Watcher<V1CustomResourceDefinition>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<Corev1Event>))]
[JsonSerializable(typeof(k8s.Watcher<Corev1Event>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Namespace>))]
[JsonSerializable(typeof(k8s.Watcher<V1Namespace>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Node>))]
[JsonSerializable(typeof(k8s.Watcher<V1Node>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1ClusterRoleBinding>))]
[JsonSerializable(typeof(k8s.Watcher<V1ClusterRoleBinding>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ClusterRole>))]
[JsonSerializable(typeof(k8s.Watcher<V1ClusterRole>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1RoleBinding>))]
[JsonSerializable(typeof(k8s.Watcher<V1RoleBinding>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Role>))]
[JsonSerializable(typeof(k8s.Watcher<V1Role>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ServiceAccount>))]
[JsonSerializable(typeof(k8s.Watcher<V1ServiceAccount>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1Secret>))]
[JsonSerializable(typeof(k8s.Watcher<V1Secret>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ConfigMap>))]
[JsonSerializable(typeof(k8s.Watcher<V1ConfigMap>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Lease>))]
[JsonSerializable(typeof(k8s.Watcher<V1Lease>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1LimitRange>))]
[JsonSerializable(typeof(k8s.Watcher<V1LimitRange>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1MutatingWebhookConfiguration>))]
[JsonSerializable(typeof(k8s.Watcher<V1MutatingWebhookConfiguration>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1PodDisruptionBudget>))]
[JsonSerializable(typeof(k8s.Watcher<V1PodDisruptionBudget>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1PriorityClass>))]
[JsonSerializable(typeof(k8s.Watcher<V1PriorityClass>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ResourceQuota>))]
[JsonSerializable(typeof(k8s.Watcher<V1ResourceQuota>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1RuntimeClass>))]
[JsonSerializable(typeof(k8s.Watcher<V1RuntimeClass>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ValidatingWebhookConfiguration>))]
[JsonSerializable(typeof(k8s.Watcher<V1ValidatingWebhookConfiguration>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1Endpoints>))]
[JsonSerializable(typeof(k8s.Watcher<V1Endpoints>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1EndpointSlice>))]
[JsonSerializable(typeof(k8s.Watcher<V1EndpointSlice>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1IngressClass>))]
[JsonSerializable(typeof(k8s.Watcher<V1IngressClass>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Ingress>))]
[JsonSerializable(typeof(k8s.Watcher<V1Ingress>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1NetworkPolicy>))]
[JsonSerializable(typeof(k8s.Watcher<V1NetworkPolicy>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Service>))]
[JsonSerializable(typeof(k8s.Watcher<V1Service>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V2HorizontalPodAutoscaler>))]
[JsonSerializable(typeof(k8s.Watcher<V2HorizontalPodAutoscaler>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1PersistentVolumeClaim>))]
[JsonSerializable(typeof(k8s.Watcher<V1PersistentVolumeClaim>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1PersistentVolume>))]
[JsonSerializable(typeof(k8s.Watcher<V1PersistentVolume>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1StorageClass>))]
[JsonSerializable(typeof(k8s.Watcher<V1StorageClass>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1Pod>))]
[JsonSerializable(typeof(k8s.Watcher<V1Pod>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1CronJob>))]
[JsonSerializable(typeof(k8s.Watcher<V1CronJob>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1DaemonSet>))]
[JsonSerializable(typeof(k8s.Watcher<V1DaemonSet>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Deployment>))]
[JsonSerializable(typeof(k8s.Watcher<V1Deployment>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Job>))]
[JsonSerializable(typeof(k8s.Watcher<V1Job>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ReplicaSet>))]
[JsonSerializable(typeof(k8s.Watcher<V1ReplicaSet>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1StatefulSet>))]
[JsonSerializable(typeof(k8s.Watcher<V1StatefulSet>.WatchEvent))]

[JsonSerializable(typeof(ExecCredentialResponse))]
[JsonSerializable(typeof(ExecCredentialResponse.ExecStatus))]

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    Converters = new[] { typeof(Iso8601TimeSpanConverter), typeof(KubernetesDateTimeConverter), typeof(KubernetesDateTimeOffsetConverter), typeof(V1Status.V1StatusObjectViewConverter) })
]
public partial class CustomSourceGenerationContext : JsonSerializerContext
{
}


