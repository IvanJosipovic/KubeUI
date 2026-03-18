using Avalonia.Styling;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.Resources
{
    public interface IResourceConfig : IInitializeCluster
    {
        bool IsNamespaced { get; }
        bool CanListAndWatch { get; }
        bool ShowNewResource { get; }
        bool IsCustomResource { get; }
        GroupApiVersionKind Kind { get; }
        IList<IResourceListColumn> Columns();
        IList<ResourceMenuItem> MenuItems();
        int Order { get; }
        string Name { get; }
        string? Category { get; }
        IStyle ListStyle();
        Task UpdatePermissions();
        Type Type { get; }
    }
}

