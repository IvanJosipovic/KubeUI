using k8s.Models;
using LiveChartsCore.Defaults;

namespace KubeUI.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitalizeCluster, INotifyPropertyChanged
{
    [ObservableProperty]
    private Client.Cluster _cluster;

    [ObservableProperty]
    private ResourceListViewModel<Corev1Event> _eventsVM;

    public ClusterViewModel()
    {
        Title = Resources.ClusterViewModel_Title;

        _eventsVM = Application.Current.GetRequiredService<ResourceListViewModel<Corev1Event>>();
    }

    [ObservableProperty]
    private ObservableValue _maxPods = new();

    [ObservableProperty]
    private ObservableValue _totalPods = new();

    [ObservableProperty]
    private ObservableValue _cpuCapacity = new();

    [ObservableProperty]
    private ObservableValue _cpuAllocatable = new();

    [ObservableProperty]
    private ObservableValue _memoryCapacity = new();

    [ObservableProperty]
    private ObservableValue _memoryAllocatable = new();

    public void RefreshData()
    {
        var pods = Cluster.GetObjectDictionary<V1Pod>();
        var nodes = Cluster.GetObjectDictionary<V1Node>();

        TotalPods.Value = pods.Count;
        MaxPods.Value = nodes.Sum(x => x.Value.Status.Capacity["pods"].ToDouble());

        CpuAllocatable.Value = nodes.Sum(x => x.Value.Status.Allocatable["cpu"].ToDouble());
        CpuCapacity.Value = nodes.Sum(x => x.Value.Status.Capacity["cpu"].ToDouble());

        MemoryAllocatable.Value = nodes.Sum(x => x.Value.Status.Allocatable["memory"].ToDouble()) / 1048576 / 1024;
        MemoryCapacity.Value = nodes.Sum(x => x.Value.Status.Capacity["memory"].ToDouble()) / 1048576 / 1024;
    }

    public void Initialize(Client.Cluster cluster)
    {
        Cluster = cluster;

        Id = nameof(ClusterViewModel) + "-" + Cluster.Name + "-" + Title;

        RefreshData();

        if (_eventsVM is IInitalizeCluster init)
        {
            init.Initialize(Cluster);
        }
    }
}
