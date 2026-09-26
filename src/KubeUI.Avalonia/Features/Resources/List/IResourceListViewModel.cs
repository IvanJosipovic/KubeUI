using Avalonia.Controls.Selection;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Resources;
using KubeUI.DynamicTableView;

namespace KubeUI.Avalonia.Features.Resources.List
{
    public interface IResourceListViewModel
    {
        ClusterWorkspace Cluster { get; set; }
        ObservableCollection<V1Namespace> SelectedNamespaces { get; }
        bool IsNamespaceSelectionLinked { get; set; }
        GroupApiVersionKind Kind { get; }
        int ItemCount { get; }
        string SearchQuery { get; set; }
        IResourceConfig ResourceConfig { get; }
        IDynamicTableViewSource TableSource { get; }
        ISelectionModel SelectionModel { get; }
        IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? selectedItems);
        DynamicTableViewState? TableViewRuntimeState { get; set; }
        void InitializeResource(ClusterWorkspace cluster, GroupApiVersionKind kind);
    }
}
