using Avalonia.Collections;
using Avalonia.Markup.Xaml.MarkupExtensions;
using k8s.Models;
using KubeUI.App.Infrastructure.DependencyInjection;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Styles;
using KubeUI.Features.Clusters.Presentation;
using KubeUI.Features.Resources.List;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;


namespace KubeUI.App.Features.Clusters.Overview;

public sealed partial class ClusterView : KubeUIView<ClusterViewModel>
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
                CreateCpuChart(),
                CreateMemoryChart(),
                CreatePodChart(),
                new Grid()
                    .Row(1)
                    .ColSpan(3)
                    .Margin(0, 12, 0, 0)
                    .Rows("Auto,*")
                    .Children(
                        new TextBlock()
                            .Row(0)
                            .FontSize(new DynamicResourceExtension(Typography.TitleFontSizeResourceKey))
                            .Text(SharedL10n.Instance.ClusterView_Events.ToBinding()),
                        new ResourceListView()
                            .Row(1)
                            .DataContext(vm.EventsVM)));
    }

    private static PieChart CreateCpuChart()
    {
        return CreateGaugeChart(
            SharedL10n.Instance.ClusterView_Cpu.CurrentValue()!,
            CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuCapacity.Value),
            new AvaloniaList<ISeries>
            {
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_CpuCapacity.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuCapacity.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_CpuAllocatable.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuAllocatable.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_CpuLimits.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuLimits.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_CpuRequests.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuRequests.Value), CPUGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_CpuUsage.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.CPUGaugeData.CpuUsage.Value), CPUGaugeData.DataLabelsFormatter),
                new XamlGaugeBackgroundSeries()
            })
            .Col(0);
    }

    private static PieChart CreateMemoryChart()
    {
        return CreateGaugeChart(
            SharedL10n.Instance.ClusterView_Memory.CurrentValue()!,
            CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryCapacity.Value),
            new AvaloniaList<ISeries>
            {
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_MemoryCapacity.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryCapacity.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_MemoryAllocatable.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryAllocatable.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_MemoryLimits.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryLimits.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_MemoryRequests.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryRequests.Value), MemoryGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_MemoryUsage.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.MemoryGaugeData.MemoryUsage.Value), MemoryGaugeData.DataLabelsFormatter),
                new XamlGaugeBackgroundSeries()
            })
            .Row(0)
            .Col(1);
    }

    private static PieChart CreatePodChart()
    {
        return CreateGaugeChart(
            SharedL10n.Instance.ClusterView_Pods.CurrentValue()!,
            CompiledBinding.Create<ClusterViewModel, double?>(x => x.PodGaugeData.MaxPods.Value),
            new AvaloniaList<ISeries>
            {
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_PodsCapacity.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.PodGaugeData.MaxPods.Value), PodGaugeData.DataLabelsFormatter),
                CreateGaugeSeries(SharedL10n.Instance.ClusterView_PodsCount.CurrentValue()!, CompiledBinding.Create<ClusterViewModel, double?>(x => x.PodGaugeData.TotalPods.Value), PodGaugeData.DataLabelsFormatter),
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
            .MaxValue(maxValueBinding)
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
        StartRefreshTimer();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StartRefreshTimer();
    }

    private void StartRefreshTimer()
    {
        if (VisualRoot == null)
        {
            return;
        }

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
