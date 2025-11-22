using Avalonia.Styling;
using k8s.Models;
using KubeUI.Client;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;

namespace KubeUI.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitializeCluster, INotifyPropertyChanged
{
    [ObservableProperty]
    public partial ISettingsService Settings { get; set; }

    [ObservableProperty]
    public partial ICluster? Cluster { get; set; }

    [ObservableProperty]
    public partial ResourceListViewModel<Corev1Event> EventsVM { get; set; }

    public ClusterViewModel()
    {
        Title = Assets.Resources.ClusterViewModel_Title;
        EventsVM = Application.Current.GetRequiredService<ResourceListViewModel<Corev1Event>>();
        Settings = Application.Current.GetRequiredService<ISettingsService>();
    }

    [ObservableProperty]
    public partial CPUGaugeData CPUGaugeData { get; set; } = new();

    [ObservableProperty]
    public partial MemoryGaugeData MemoryGaugeData { get; set; } = new();

    [ObservableProperty]
    public partial PodGaugeData PodGaugeData { get; set; } = new();

    public async Task RefreshData()
    {
        if (Cluster == null)
        {
            return;
        }

        await Cluster.SeedResource<V1Pod>();
        await Cluster.IsResourceReady<V1Pod>();
        await Cluster.SeedResource<V1Node>();
        await Cluster.IsResourceReady<V1Node>();
        var pods = Cluster.GetResourceList<V1Pod>();
        var nodes = Cluster.GetResourceList<V1Node>();

        PodGaugeData.TotalPods.Value = pods.Count;
        PodGaugeData.MaxPods.Value = nodes.Sum(x => x.Status.Capacity?.TryGetValue("pods", out var value) == true ? value.ToDouble() : 0);

        CPUGaugeData.CpuAllocatable.Value = nodes.Sum(x => x.Status.Allocatable?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0);
        CPUGaugeData.CpuCapacity.Value = nodes.Sum(x => x.Status.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0);
        CPUGaugeData.CpuRequests.Value = pods.Sum(x => x.Spec.Containers.Sum(y => y.Resources?.Requests?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0));
        CPUGaugeData.CpuLimits.Value = pods.Sum(x => x.Spec.Containers.Sum(y => y.Resources?.Limits?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0));
        CPUGaugeData.CpuUsage.Value = Cluster.PodMetrics.Sum(x => x.Containers.Sum(y => y.Usage?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0));

        MemoryGaugeData.MemoryAllocatable.Value = nodes.Sum(x => x.Status.Allocatable?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0) / 1048576 / 1024;
        MemoryGaugeData.MemoryCapacity.Value = nodes.Sum(x => x.Status.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0) / 1048576 / 1024;
        MemoryGaugeData.MemoryRequests.Value = pods.Sum(x => x.Spec.Containers.Sum(y => y.Resources?.Requests?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0)) / 1048576 / 1024;
        MemoryGaugeData.MemoryLimits.Value = pods.Sum(x => x.Spec.Containers.Sum(y => y.Resources?.Limits?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0)) / 1048576 / 1024;
        MemoryGaugeData.MemoryUsage.Value = Cluster.PodMetrics.Sum(x => x.Containers.Sum(y => y.Usage?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0)) / 1048576 / 1024;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;

        Id = nameof(ClusterViewModel) + "-" + Cluster.Name + "-" + Title;

        Cluster.SeedResource<Corev1Event>().GetAwaiter().GetResult();

        if (EventsVM is IInitializeCluster init)
        {
            init.Initialize(Cluster);
        }
    }

    public static string TextColor
    {
        get
        {
            if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                return "#FFFFFF";
            }

            return "#000000";
        }
    }
}

public partial class CPUGaugeData : ObservableObject
{
    [ObservableProperty]
    public partial ObservableValue CpuCapacity { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuAllocatable { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuLimits { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuRequests { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuUsage { get; set; } = new();

    public static Func<ChartPoint, string> DataLabelsFormatter { get; } = p => $"{p.Coordinate.PrimaryValue:F2}c";
}

public partial class MemoryGaugeData : ObservableObject
{
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

    public static Func<ChartPoint, string> DataLabelsFormatter { get; } = p => $"{p.Coordinate.PrimaryValue:F2}Gi";

}

public partial class PodGaugeData : ObservableObject
{
    [ObservableProperty]
    public partial ObservableValue MaxPods { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue TotalPods { get; set; } = new();

    public static Func<ChartPoint, string> DataLabelsFormatter { get; } = p => $"{p.Coordinate.PrimaryValue:F0}";
}
