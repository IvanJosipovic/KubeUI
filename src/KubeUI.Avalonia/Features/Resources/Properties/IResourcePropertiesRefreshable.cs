using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Features.Resources.Properties;

internal interface IResourcePropertiesRefreshable<T>
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    void Refresh(T resource);
}
