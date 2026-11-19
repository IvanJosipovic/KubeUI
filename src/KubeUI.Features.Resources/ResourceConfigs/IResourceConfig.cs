using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Common;

namespace KubeUI.Features.Resources
{
    public interface IResourceConfig : IClusterResourceConfig
    {
        bool ShowNewResource { get; }

        /// <summary>Gets whether this resource configuration is backed by a custom resource definition.</summary>
        bool IClusterResourceConfig.IsCustomResource => Origin is ResourceOrigin.CustomResourceDefinition or ResourceOrigin.ApiDiscovery;

        /// <summary>Gets where this kind's model comes from.</summary>
        ResourceOrigin Origin { get; }

        new

                /// <summary>Gets the defining CRD for a discovered kind, or null for a built-in kind.</summary>
                V1CustomResourceDefinition? CustomResourceDefinition => null;

        /// <summary>Gets the canonical location of this kind in cluster navigation.</summary>
        NavigationPlacement Navigation { get; }
        IList<IResourceListColumn> Columns();
        IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems);
        IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems);

        new string Name { get; }
        Style[] ListStyle();
        IRelayCommand NewResourceCommand { get; }
        IRelayCommand<IList> ViewCommand { get; }
        IAsyncRelayCommand<IList> DeleteCommand { get; }
    }
}
