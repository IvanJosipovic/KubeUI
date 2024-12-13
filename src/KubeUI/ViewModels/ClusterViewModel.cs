using k8s.Models;
using KubeUI.Client;
using LiveChartsCore.Defaults;

namespace KubeUI.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitializeCluster, INotifyPropertyChanged
{
    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial ResourceListViewModel<Corev1Event> EventsVM { get; set; }

    public ClusterViewModel()
    {
        Title = Resources.ClusterViewModel_Title;
        EventsVM = Application.Current.GetRequiredService<ResourceListViewModel<Corev1Event>>();
    }

    [ObservableProperty]
    public partial ObservableValue MaxPods { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue TotalPods { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuCapacity { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuAllocatable { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuRequests { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuLimits { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuUsage { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryCapacity { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryAllocatable { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryRequests { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryLimits { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryUsage { get; set; } = new();

    public void RefreshData()
    {
        var pods = Cluster.GetObjectDictionary<V1Pod>();
        var nodes = Cluster.GetObjectDictionary<V1Node>();

        TotalPods.Value = pods.Count;
        MaxPods.Value = nodes.Sum(x => x.Value.Status.Capacity?.TryGetValue("pods", out var value) == true ? value.ToDouble(): 0);

        CpuAllocatable.Value = nodes.Sum(x => x.Value.Status.Allocatable?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0);
        CpuCapacity.Value = nodes.Sum(x => x.Value.Status.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0);
        CpuRequests.Value = pods.Sum(x => x.Value.Spec.Containers.Sum(y => y.Resources?.Requests?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0));
        CpuLimits.Value = pods.Sum(x => x.Value.Spec.Containers.Sum(y => y.Resources?.Limits?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0));
        CpuUsage.Value = Cluster.PodMetrics.Sum(x => x.Containers.Sum(y => y.Usage?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0));

        MemoryAllocatable.Value = nodes.Sum(x => x.Value.Status.Allocatable?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0) / 1048576 / 1024;
        MemoryCapacity.Value = nodes.Sum(x => x.Value.Status.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0) / 1048576 / 1024;
        MemoryRequests.Value = pods.Sum(x => x.Value.Spec.Containers.Sum(y => y.Resources?.Requests?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0)) / 1048576 / 1024;
        MemoryLimits.Value = pods.Sum(x => x.Value.Spec.Containers.Sum(y => y.Resources?.Limits?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0)) / 1048576 / 1024;
        MemoryUsage.Value = Cluster.PodMetrics.Sum(x => x.Containers.Sum(y => y.Usage?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0)) / 1048576 / 1024;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;

        Id = nameof(ClusterViewModel) + "-" + Cluster.Name + "-" + Title;

        RefreshData();

        if (EventsVM is IInitializeCluster init)
        {
            init.Initialize(Cluster);
        }
    }
}
