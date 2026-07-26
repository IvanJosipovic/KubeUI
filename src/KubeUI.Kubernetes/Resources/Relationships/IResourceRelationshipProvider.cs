using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships;

public interface IResourceRelationshipProvider
{
    void AddRelationships(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships);
}
