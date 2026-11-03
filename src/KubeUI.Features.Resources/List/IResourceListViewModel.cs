using Avalonia.Controls.Selection;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Common;
using KubeUI.Features.Resources;

namespace KubeUI.Features.Resources.List
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
        /// <summary>Stores selected resource keys so a recreated list view can restore visible selections.</summary>
        void CaptureSelectionState();
        /// <summary>Restores captured selections for resources that remain visible in the table source.</summary>
        void RestoreSelectionState();
        void InitializeResource(ClusterWorkspace cluster, GroupApiVersionKind kind);
    }
}
