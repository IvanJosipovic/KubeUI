namespace KubeUI.Features.Resources.Icons;

using KubernetesClient.Informer.Client;

public interface IResourceIconService
{
    IImage GetIcon(GroupApiVersionKind resourceKind);
}
