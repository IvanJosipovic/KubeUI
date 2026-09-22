using k8s;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public readonly record struct ResourceChange(
    WatchEventType EventType,
    GroupApiVersionKind Kind,
    IKubernetesObject<k8s.Models.V1ObjectMeta> Resource);
