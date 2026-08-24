namespace KubeUI.Avalonia.Services.Icons;

using KubernetesClient.Informer.Client;

public interface IResourceIconService
{
    IImage GetIcon(GroupApiVersionKind resourceKind);
}
