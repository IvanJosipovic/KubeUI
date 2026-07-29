using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void resource_count_assigned_after_template_creation_is_rendered()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(TestApp.CurrentServices!, runtime);
        using var navigation = TestApp.CurrentServices!.GetRequiredService<NavigationViewModel>();
        var clusterNode = new ClusterNavigationNode(workspace) { IsExpanded = true };
        var podsLink = new ResourceNavigationLink
        {
            Cluster = workspace,
            Id = "test-pods",
            Name = "Pods",
            ControlType = typeof(V1Pod),
        };
        clusterNode.NavigationItems.Add(podsLink);
        navigation.Clusters.Add(clusterNode);

        var view = new NavigationView { DataContext = navigation };
        var window = new Window { Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        podsLink.Count = Observable.Return(3);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).ShouldContain("3");

        window.Content = null;
        window.Close();
    }
}
