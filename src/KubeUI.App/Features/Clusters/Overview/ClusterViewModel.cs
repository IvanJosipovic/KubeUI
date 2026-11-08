using k8s.Models;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.List;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;

namespace KubeUI.App.Features.Clusters.Overview;

public sealed partial class ClusterViewModel : ViewModelBase, IInitializeCluster, INotifyPropertyChanged
{
    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial ResourceListViewModel<Corev1Event> EventsVM { get; set; }

    public ClusterViewModel(
        ResourceListViewModel<Corev1Event> eventsVm)
    {
        Title = AppLanguageManager.Instance.ClusterView_Title.CurrentValue()!;
        EventsVM = eventsVm;
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

        await Cluster.Runtime.SeedResource<V1Pod>(true);
        await Cluster.Runtime.SeedResource<V1Node>(true);
        var snapshot = ClusterMetricSnapshotBuilder.Build(
            Cluster.Runtime.GetResourceList<V1Pod>(),
            Cluster.Runtime.GetResourceList<V1Node>(),
            Cluster.Runtime.PodMetrics);

        PodGaugeData.TotalPods.Value = snapshot.TotalPods;
        PodGaugeData.MaxPods.Value = snapshot.MaxPods;

        CPUGaugeData.CpuAllocatable.Value = snapshot.CpuAllocatable;
        CPUGaugeData.CpuCapacity.Value = snapshot.CpuCapacity;
        CPUGaugeData.CpuRequests.Value = snapshot.CpuRequests;
        CPUGaugeData.CpuLimits.Value = snapshot.CpuLimits;
        CPUGaugeData.CpuUsage.Value = snapshot.CpuUsage;

        MemoryGaugeData.MemoryAllocatable.Value = snapshot.MemoryAllocatableGi;
        MemoryGaugeData.MemoryCapacity.Value = snapshot.MemoryCapacityGi;
        MemoryGaugeData.MemoryRequests.Value = snapshot.MemoryRequestsGi;
        MemoryGaugeData.MemoryLimits.Value = snapshot.MemoryLimitsGi;
        MemoryGaugeData.MemoryUsage.Value = snapshot.MemoryUsageGi;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;

        Id = nameof(ClusterViewModel) + "-" + Cluster.Runtime.Name + "-" + Title;

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
