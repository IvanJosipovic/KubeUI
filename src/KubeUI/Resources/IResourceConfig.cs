using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Resources
{
    public interface IResourceConfig
    {
        string? Category { get; }
        bool DefaultMenuItems { get; }
        GroupApiVersionKind GroupApiVersionKind { get; }
        Func<StyleGroup>? SetStyle { get; set; }
        bool ShowNamespaces { get; }
        bool ShowNewResource { get; }
        Type Type { get; }
        int Order { get; }
        string Name { get; }

        IList<IResourceListViewDefinitionColumn> Columns();
        IList<ResourceListViewMenuItem> MenuItems();
    }
}
