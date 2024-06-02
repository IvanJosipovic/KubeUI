using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ClusterManager _clusterManager;

    public ClusterListViewModel()
    {
        _clusterManager = Application.Current.GetRequiredService<ClusterManager>();
    }
}
