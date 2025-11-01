using Avalonia.Styling;
using KubeUI.Client;

namespace KubeUI.Resources
{
    public interface IResourceConfig : IInitializeCluster
    {
        bool IsNamespaced { get; }
        bool ShowNewResource { get; }
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
