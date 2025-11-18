using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace KubeUI.Views;

public sealed partial class ClusterView : UserControl
{
    private readonly DispatcherTimer _timer = new();
    public ClusterViewModel? ViewModel => DataContext as ClusterViewModel;

    public ClusterView()
    {
        InitializeComponent();

        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += TimerOnTick;
        _timer.Start();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterManager>().GetDefault();
                await cluster.Connect();
                await cluster.SeedResource<Corev1Event>();

                var vm = Application.Current.GetRequiredService<ClusterViewModel>() as IDockable;

                if (vm is IInitializeCluster init)
                {
                    init.Initialize(cluster);
                }

                DataContext = vm;
            });
        }
#endif
    }

    private async void TimerOnTick(object? sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.RefreshData();
            }
            catch
            {
                // Swallow refresh exceptions to keep timer going.
            }
        }
    }

    private static void ConfigureSeries(string name, PieSeries<ObservableValue> series)
    {
        series.Name = name;
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.InnerRadius = 20;
        series.RelativeOuterRadius = 8;
        series.RelativeInnerRadius = 8;
        series.ToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue}";
        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}";
    }

    private void BuildCharts()
    {
        if (ViewModel == null)
            return;

        // CPU
        //CpuChart.Title = MakeTitle("CPU");
        //CpuChart.MaxValue = ViewModel.CpuCapacity.Value;
        //CpuChart.Series = GaugeGenerator.BuildSolidGauge(
        //    new GaugeItem(ViewModel.CpuCapacity, s =>
        //    {
        //        ConfigureSeries("Capacity", s);
        //        s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //        s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //    }),
        //    new GaugeItem(ViewModel.CpuAllocatable, s =>
        //    {
        //        ConfigureSeries("Allocatable", s);
        //        s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //        s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //    }),
        //    new GaugeItem(ViewModel.CpuLimits, s =>
        //    {
        //        ConfigureSeries("Limits", s);
        //        s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //        s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //    }),
        //    new GaugeItem(ViewModel.CpuRequests, s =>
        //    {
        //        ConfigureSeries("Requests", s);
        //        s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //        s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //    }),
        //    new GaugeItem(ViewModel.CpuUsage, s =>
        //    {
        //        ConfigureSeries("Usage", s);
        //        s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //        s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F2}c";
        //    })
        //);

        // Memory
        //MemoryChart.Title = MakeTitle("Memory");
        //MemoryChart.MaxValue = ViewModel.MemoryCapacity.Value;
        MemoryChart.Series = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(ViewModel.MemoryCapacity, s =>
            {
                ConfigureSeries("Capacity", s);
                s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
                s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
            }),
            new GaugeItem(ViewModel.MemoryAllocatable, s =>
            {
                ConfigureSeries("Allocatable", s);
                s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
                s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
            }),
            new GaugeItem(ViewModel.MemoryLimits, s =>
            {
                ConfigureSeries("Limits", s);
                s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}Gi";
                s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
            }),
            new GaugeItem(ViewModel.MemoryRequests, s =>
            {
                ConfigureSeries("Requests", s);
                s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}Gi";
                s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
            }),
            new GaugeItem(ViewModel.MemoryUsage, s =>
            {
                ConfigureSeries("Usage", s);
                s.DataLabelsFormatter = p => $"{p.Coordinate.PrimaryValue:F2}Gi";
                s.ToolTipLabelFormatter = p => $"{p.Coordinate.PrimaryValue:F1}Gi";
            })
        );

        // Pods
        //PodsChart.Title = MakeTitle("Pods");
        //PodsChart.MaxValue = ViewModel.MaxPods.Value;
        PodsChart.Series = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(ViewModel.MaxPods, s => ConfigureSeries("Capacity", s)),
            new GaugeItem(ViewModel.TotalPods, s => ConfigureSeries("Count", s))
        );
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        BuildCharts();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _timer.Stop();
        _timer.Tick -= TimerOnTick;
    }
}
