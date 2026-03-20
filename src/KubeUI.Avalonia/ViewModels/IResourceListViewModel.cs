using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using KubeUI.Avalonia.Resources;
using KubernetesClient.Informer.Client;
using Avalonia.Controls.DataGridSearching;

namespace KubeUI.Avalonia.ViewModels
{
    public interface IResourceListViewModel
    {
        ClusterWorkspaceViewModel Cluster { get; set; }
        GroupApiVersionKind Kind { get; }
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
        IEnumerable<MenuItemViewModel> ContextMenuItems { get; }
        ISearchModel SearchModel { get; set; }
        IDataGridSearchAdapterFactory SearchAdapterFactory { get; }
        // Runtime DataGrid state captured from ProDataGrid (in-memory snapshot)
        DataGridState? DataGridRuntimeState { get; set; }
    }
}



