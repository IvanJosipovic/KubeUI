using KubeUI.Client;

namespace KubeUI.Resources
{
    public interface IResourceConfig: IInitializeCluster
    {
        string? Category { get; }
        bool DefaultMenuItems { get; }
        GroupApiVersionKind Kind { get; }

        bool ShowNamespaces { get; }
        bool ShowNewResource { get; }
        Type Type { get; }
        int Order { get; }
        string Name { get; }
        StyleGroup ListStyle();
        IList<IResourceListColumn> Columns();
        IList<ResourceMenuItem> MenuItems();
    }
}
