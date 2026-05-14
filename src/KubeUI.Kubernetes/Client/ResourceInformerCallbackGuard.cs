using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

internal static class ResourceInformerCallbackGuard
{
    internal static void Execute<T>(
        ILogger logger,
        WatchEventType eventType,
        GroupApiVersionKind kind,
        T item,
        Action action) where T : class, IKubernetesObject<V1ObjectMeta>
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing {WatchEventType} notification for {ResourceType} {ItemKind}/{ItemName}.{ItemNamespace} at resource version {ResourceVersion}.",
                eventType,
                kind,
                item.Kind,
                item.Name(),
                item.Namespace(),
                item.ResourceVersion());
        }
    }
}
