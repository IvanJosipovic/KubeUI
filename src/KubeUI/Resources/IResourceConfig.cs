using KubeUI.Client;

namespace KubeUI.Resources
{
    public interface IResourceConfig: IInitializeCluster
    {
        bool IsNamespaced { get; }
        bool ShowNewResource { get; }
        GroupApiVersionKind Kind { get; }
        IList<IResourceListColumn> Columns();
        IList<ResourceMenuItem> MenuItems();
        int Order { get; }
        string Name { get; }
        string? Category { get; }
        StyleGroup ListStyle();
        Task UpdatePermissions();
        Type Type { get; }
    }
}
