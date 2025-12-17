using Avalonia.Collections;
using k8s;
using k8s.Models;
using KubeUI.Client;
using Yarp.Kubernetes.Controller.Client;
using KubeUI.Resources;

namespace KubeUI.ViewModels
{
    public interface IResourceListViewModel
    {
        ICluster Cluster { get; set; }
        GroupApiVersionKind Kind { get; set; }
        string SearchQuery { get; set; }
        ISettingsService SettingsService { get; }
        DataGridCollectionView ItemsView { get; }
        ObservableCollection<DataGridColumn> Columns { get; set; }

        void Dispose();
        void Initialize(ICluster cluster);
    }
}
