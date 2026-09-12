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
[JsonSerializable(typeof(Watcher<KubernetesObject>.WatchEvent))]
[JsonSerializable(typeof(KeyValuePair<object, object>))]
[JsonSerializable(typeof(KubernetesObject))]
[JsonSerializable(typeof(GenericKubernetesObject))]
[JsonSerializable(typeof(KubernetesList<GenericKubernetesObject>))]
[JsonSerializable(typeof(Watcher<GenericKubernetesObject>.WatchEvent))]
[JsonSerializable(typeof(ValueType))]
[JsonSerializable(typeof(WatchEventType))]
[JsonSerializable(typeof(IReadOnlyDictionary<object, object>))]
[JsonSerializable(typeof(IReadOnlyCollection<KeyValuePair<object, object>>))]
[JsonSerializable(typeof(IDeserializationCallback))]

[JsonSerializable(typeof(KubernetesList<V1CustomResourceDefinition>))]
[JsonSerializable(typeof(Watcher<V1CustomResourceDefinition>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<Corev1Event>))]
[JsonSerializable(typeof(Watcher<Corev1Event>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Namespace>))]
[JsonSerializable(typeof(Watcher<V1Namespace>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Node>))]
[JsonSerializable(typeof(Watcher<V1Node>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1ClusterRoleBinding>))]
[JsonSerializable(typeof(Watcher<V1ClusterRoleBinding>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ClusterRole>))]
[JsonSerializable(typeof(Watcher<V1ClusterRole>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1RoleBinding>))]
[JsonSerializable(typeof(Watcher<V1RoleBinding>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Role>))]
[JsonSerializable(typeof(Watcher<V1Role>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ServiceAccount>))]
[JsonSerializable(typeof(Watcher<V1ServiceAccount>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1Secret>))]
[JsonSerializable(typeof(Watcher<V1Secret>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ConfigMap>))]
[JsonSerializable(typeof(Watcher<V1ConfigMap>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Lease>))]
[JsonSerializable(typeof(Watcher<V1Lease>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1LimitRange>))]
[JsonSerializable(typeof(Watcher<V1LimitRange>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1MutatingWebhookConfiguration>))]
[JsonSerializable(typeof(Watcher<V1MutatingWebhookConfiguration>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1PodDisruptionBudget>))]
[JsonSerializable(typeof(Watcher<V1PodDisruptionBudget>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1PriorityClass>))]
[JsonSerializable(typeof(Watcher<V1PriorityClass>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ResourceQuota>))]
[JsonSerializable(typeof(Watcher<V1ResourceQuota>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1RuntimeClass>))]
[JsonSerializable(typeof(Watcher<V1RuntimeClass>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ValidatingWebhookConfiguration>))]
[JsonSerializable(typeof(Watcher<V1ValidatingWebhookConfiguration>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1Endpoints>))]
[JsonSerializable(typeof(Watcher<V1Endpoints>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1EndpointSlice>))]
[JsonSerializable(typeof(Watcher<V1EndpointSlice>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1IngressClass>))]
[JsonSerializable(typeof(Watcher<V1IngressClass>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Ingress>))]
[JsonSerializable(typeof(Watcher<V1Ingress>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1NetworkPolicy>))]
[JsonSerializable(typeof(Watcher<V1NetworkPolicy>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Service>))]
[JsonSerializable(typeof(Watcher<V1Service>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V2HorizontalPodAutoscaler>))]
[JsonSerializable(typeof(Watcher<V2HorizontalPodAutoscaler>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1PersistentVolumeClaim>))]
[JsonSerializable(typeof(Watcher<V1PersistentVolumeClaim>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1PersistentVolume>))]
[JsonSerializable(typeof(Watcher<V1PersistentVolume>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1StorageClass>))]
[JsonSerializable(typeof(Watcher<V1StorageClass>.WatchEvent))]

[JsonSerializable(typeof(KubernetesList<V1Pod>))]
[JsonSerializable(typeof(Watcher<V1Pod>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1CronJob>))]
[JsonSerializable(typeof(Watcher<V1CronJob>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1DaemonSet>))]
[JsonSerializable(typeof(Watcher<V1DaemonSet>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Deployment>))]
[JsonSerializable(typeof(Watcher<V1Deployment>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1Job>))]
[JsonSerializable(typeof(Watcher<V1Job>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1ReplicaSet>))]
[JsonSerializable(typeof(Watcher<V1ReplicaSet>.WatchEvent))]
[JsonSerializable(typeof(KubernetesList<V1StatefulSet>))]
[JsonSerializable(typeof(Watcher<V1StatefulSet>.WatchEvent))]

[JsonSerializable(typeof(ExecCredentialResponse))]
[JsonSerializable(typeof(ExecCredentialResponse.ExecStatus))]
[JsonSerializable(typeof(V1SelfSubjectAccessReview))]
[JsonSerializable(typeof(V1SelfSubjectAccessReviewSpec))]
[JsonSerializable(typeof(V1NonResourceAttributes))]
[JsonSerializable(typeof(V1ResourceAttributes))]
[JsonSerializable(typeof(V1FieldSelectorAttributes))]
[JsonSerializable(typeof(IList<V1FieldSelectorRequirement>))]
[JsonSerializable(typeof(V1FieldSelectorRequirement))]
[JsonSerializable(typeof(V1LabelSelectorAttributes))]
[JsonSerializable(typeof(V1SubjectAccessReviewStatus))]
[JsonSerializable(typeof(V1APIGroupList))]
[JsonSerializable(typeof(IList<V1APIGroup>))]
[JsonSerializable(typeof(V1APIGroup))]
[JsonSerializable(typeof(V1GroupVersionForDiscovery))]
[JsonSerializable(typeof(IList<V1ServerAddressByClientCIDR>))]
[JsonSerializable(typeof(V1ServerAddressByClientCIDR))]
[JsonSerializable(typeof(IList<V1GroupVersionForDiscovery>))]

[JsonSerializable(typeof(V2beta1APIGroupDiscoveryList))]

[JsonSerializable(typeof(Watcher<V1Status>.WatchEvent))]

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    Converters = new[] { typeof(Iso8601TimeSpanConverter), typeof(KubernetesDateTimeConverter), typeof(KubernetesDateTimeOffsetConverter), typeof(V1Status.V1StatusObjectViewConverter) })
]
public partial class CustomSourceGenerationContext : JsonSerializerContext
{
}
