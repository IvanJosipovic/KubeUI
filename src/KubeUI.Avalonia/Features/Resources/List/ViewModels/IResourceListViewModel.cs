using System.Collections;
using Avalonia.Collections;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.List.ViewModels
{
    public interface IResourceListViewModel
    {
        ClusterWorkspaceViewModel Cluster { get; set; }
        ObservableCollection<V1Namespace> SelectedNamespaces { get; }
        bool IsNamespaceSelectionLinked { get; set; }
        GroupApiVersionKind Kind { get; }
        int ItemCount { get; }
        string SearchQuery { get; set; }
        ISettingsService SettingsService { get; }
        IResourceConfig ResourceConfig { get; }
        ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }
        IDataGridSortingAdapterFactory SortingAdapterFactory { get; }
        ISortingModel SortingModel { get; set; }
        IDataGridFilteringAdapterFactory FilteringAdapterFactory { get; }
        IFilteringModel FilteringModel { get; set; }
        ISelectionModel SelectionModel { get; }
        Func<IList, object, int> ReferenceIndexResolver { get; }
        IList View { get; }
        IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? selectedItems);
        ISearchModel SearchModel { get; set; }
        IDataGridSearchAdapterFactory SearchAdapterFactory { get; }
        // Runtime DataGrid state captured from ProDataGrid (in-memory snapshot)
        DataGridState? DataGridRuntimeState { get; set; }
    }
}



