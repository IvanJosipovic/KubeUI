using Avalonia.Collections;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSorting;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using Yarp.Kubernetes.Controller.Client;

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

        void Dispose();
        void Initialize(ICluster cluster);
    }
}
