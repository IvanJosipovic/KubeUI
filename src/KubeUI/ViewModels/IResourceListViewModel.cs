using Avalonia.Collections;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSelection;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using Kubernetes.Controller.Client;
using Avalonia.Controls.DataGridSearching;

namespace KubeUI.ViewModels
{
    public interface IResourceListViewModel
    {
        ICluster Cluster { get; set; }
        GroupApiVersionKind Kind { get; }
        string SearchQuery { get; set; }
        ISettingsService SettingsService { get; }
        ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }
        IDataGridSortingAdapterFactory SortingAdapterFactory { get; }
        ISortingModel SortingModel { get; set; }
        IDataGridFilteringAdapterFactory FilteringAdapterFactory { get; }
        IFilteringModel FilteringModel { get; set; }
        ISelectionModel SelectionModel { get; }
        IDataGridSelectionModelFactory SelectionModelFactory { get; }
        IEnumerable View { get; }
        ISearchModel SearchModel { get; set; }
        IDataGridSearchAdapterFactory SearchAdapterFactory { get; }
    }
}
