using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Features.Clusters.Overview;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Overview;

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
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        GetRefreshTimer(view).IsEnabled.ShouldBeTrue();

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();
        GetRefreshTimer(view).IsEnabled.ShouldBeFalse();

        window.Content = view;
        await TestApplicationExtensions.WaitForUiAsync();

        GetRefreshTimer(view).IsEnabled.ShouldBeTrue();
        window.Close();
    }

    private static DispatcherTimer GetRefreshTimer(ClusterView view)
    {
        return ((DispatcherTimer?)typeof(ClusterView)
            .GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(view)).ShouldNotBeNull();
    }
}
