using Avalonia.Collections;
using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Resources.List.Views;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Views;

public sealed partial class ClusterView : ViewBase<ClusterViewModel>
{
    private readonly DispatcherTimer _timer = new();

    public ClusterView()
    {
        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    protected override object Build(ClusterViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Margin(8)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Cols("*,*,*")
            .Rows("*,2*")
            .Children(
                CreateCpuChart(vm),
                CreateMemoryChart(vm),
                CreatePodChart(vm),
                new Grid()
                    .Row(1)
                    .ColSpan(3)
                    .Margin(0, 12, 0, 0)
                    .Rows("Auto,*")
                    .Children(
                        new TextBlock()
                            .Row(0)
                            .FontSize(25)
                            .Text(Assets.Resources.ClusterView_Events),
                        new ResourceListView()
                            .Row(1)
                            .DataContext(vm.EventsVM)));
    }

    private static PieChart CreateCpuChart(ClusterViewModel vm)
    {
        return CreateGaugeChart(
            Assets.Resources.ClusterView_Cpu,
            CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuCapacity.Value),
            new AvaloniaList<ISeries>
            {
                CreateGaugeSeries(Assets.Resources.ClusterView_CpuCapacity, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuCapacity.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_CpuAllocatable, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuAllocatable.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_CpuLimits, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuLimits.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_CpuRequests, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuRequests.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_CpuUsage, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuUsage.Value), CPUGaugeData.DataLabelsFormatter),
                new XamlGaugeBackgroundSeries()
            })
            .Col(0);
    }

    private static PieChart CreateMemoryChart(ClusterViewModel vm)
    {
        return CreateGaugeChart(
            Assets.Resources.ClusterView_Memory,
            CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryCapacity.Value),
            new AvaloniaList<ISeries>
            {
                CreateGaugeSeries(Assets.Resources.ClusterView_MemoryCapacity, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryCapacity.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_MemoryAllocatable, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryAllocatable.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_MemoryLimits, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryLimits.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_MemoryRequests, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryRequests.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_MemoryUsage, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryUsage.Value), MemoryGaugeData.DataLabelsFormatter),
                new XamlGaugeBackgroundSeries()
            })
            .Row(0)
            .Col(1);
    }

    private static PieChart CreatePodChart(ClusterViewModel vm)
    {
        return CreateGaugeChart(
            Assets.Resources.ClusterView_Pods,
            CompiledBinding.Create<ClusterViewModel, double?>(x => x.PodGaugeData.MaxPods.Value),
            new AvaloniaList<ISeries>
            {
                CreateGaugeSeries(Assets.Resources.ClusterView_PodsCapacity, CompiledBinding.Create<ClusterViewModel, double?>(x => x.PodGaugeData.MaxPods.Value), PodGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(Assets.Resources.ClusterView_PodsCount, CompiledBinding.Create<ClusterViewModel, double?>(x => x.PodGaugeData.TotalPods.Value), PodGaugeData.DataLabelsFormatter),
                new XamlGaugeBackgroundSeries()
            })
            .Row(0)
            .Col(2);
    }

    private static PieChart CreateGaugeChart(string title, BindingBase maxValueBinding, AvaloniaList<ISeries> series)
    {
        return new PieChart()
            .MinHeight(100)
            .LegendTextSize(10)
            .InitialRotation(-90)
            .LegendPosition(LegendPosition.Bottom)
            .MaxAngle(270)
            .BindValue(PieChart.MaxValueProperty, maxValueBinding)
            .Title(new XamlDrawnLabelVisual
            {
                Paint = new SolidColorPaint(SKColor.Parse(ClusterViewModel.TextColor)),
                Text = title,
                TextSize = 25
            })
            .Series(series);
    }

    private static XamlGaugeSeries CreateGaugeSeries(string name, BindingBase gaugeValueBinding, Func<ChartPoint, string> formatter)
    {
        return new XamlGaugeSeries
        {
            DataLabelsFormatter = formatter,
            DataLabelsPosition = PolarLabelsPosition.Start,
            DataLabelsSize = 10,
            InnerRadius = 20,
            SeriesName = name,
            ToolTipLabelFormatter = formatter
        }
            .BindValue(XamlGaugeSeries.GaugeValueProperty, gaugeValueBinding);
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ClusterViewModel, V1Pod>();
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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (!_timer.IsEnabled)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _timer.Stop();
        _timer.Tick -= TimerOnTick;
    }
}



