using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.App.Features.Clusters.Overview;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Shouldly;

namespace KubeUI.App.Tests.App.Features.Clusters.Overview;

public sealed class ClusterViewTests
{
    [AvaloniaFact]
    public async Task cluster_view_restarts_refresh_timer_after_recycling()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var viewModel = Application.Current.GetRequiredTestService<ClusterViewModel>();
        viewModel.Initialize(cluster);

        var view = new ClusterView { ViewModel = viewModel };
        var window = new Window { Content = view };
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();

            GetRefreshTimer(view).IsEnabled.ShouldBeTrue();

            window.Content = null;
            await TestApplicationExtensions.WaitForUiAsync();
            GetRefreshTimer(view).IsEnabled.ShouldBeFalse();

            view.DataContext = null;
            view.DataContext = viewModel;
            await TestApplicationExtensions.WaitForUiAsync();
            GetRefreshTimer(view).IsEnabled.ShouldBeFalse();

            window.Content = view;
            await TestApplicationExtensions.WaitForUiAsync();

            GetRefreshTimer(view).IsEnabled.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task cluster_gauges_use_flat_arc_ends()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var viewModel = Application.Current.GetRequiredTestService<ClusterViewModel>();
        viewModel.Initialize(cluster);

        var view = new ClusterView { ViewModel = viewModel };
        var window = new Window { Content = view };
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();

            var charts = view.GetVisualDescendants().OfType<PieChart>().ToList();
            charts.Count.ShouldBe(3);
            foreach (var chart in charts)
            {
                var series = chart.Series.OfType<IPieSeries>().ToList();
                series.ShouldNotBeEmpty();
                series.ShouldAllBe(x => x.CornerRadius == 0);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static DispatcherTimer GetRefreshTimer(ClusterView view)
    {
        return ((DispatcherTimer?)typeof(ClusterView)
            .GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(view)).ShouldNotBeNull();
    }
}
