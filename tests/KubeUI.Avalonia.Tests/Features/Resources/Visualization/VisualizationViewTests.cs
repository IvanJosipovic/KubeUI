using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;
using KubeUI.Avalonia.Features.Resources.Visualization.Views;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class VisualizationViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task namespace_selector_hides_when_visualizing_a_resource()
    {
        var cluster = await TestCluster.GetAsync();

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var viewModel = services.GetRequiredService<VisualizationViewModel>();
        var view = new VisualizationView
        {
            DataContext = viewModel,
        };

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var namespaceSelector = view.FindControl<Control>("NamespaceSelector");
        var resourceToolbar = view.FindControl<Control>("ResourceToolbarText");
        namespaceSelector.ShouldNotBeNull();
        resourceToolbar.ShouldNotBeNull();
        var resourceToolbarText = view.FindControl<TextBlock>("ResourceToolbarText");
        resourceToolbarText.ShouldNotBeNull();

        viewModel.Initialize(cluster);
        Dispatcher.UIThread.RunJobs();
        namespaceSelector.IsVisible.ShouldBeTrue();
        resourceToolbar.IsVisible.ShouldBeFalse();

        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec()
        };

        await cluster.AddOrUpdateResource(pod);

        viewModel.Initialize(cluster, pod);
        Dispatcher.UIThread.RunJobs();

        namespaceSelector.IsVisible.ShouldBeFalse();
        resourceToolbar.IsVisible.ShouldBeTrue();
        resourceToolbarText.Text.ShouldBe("Resource: v1/Pod default/pod-1");

        window.Content = null;
        window.Close();
    }

    [AvaloniaFact]
    public async Task resource_nodes_use_resource_config_context_menu_items()
    {
        var cluster = await TestCluster.GetAsync();
        await cluster.EnsureWorkspaceStateInitializedAsync();

        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox",
                    }
                ],
            },
        };

        await cluster.AddOrUpdateResource(pod);

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var viewModel = services.GetRequiredService<VisualizationViewModel>();
        viewModel.Initialize(cluster, pod);

        var node = viewModel.Resources.Single(x => ReferenceEquals(x.Resource, pod));
        var expectedDefaultHeaders = cluster
            .GetResourceConfig<V1Pod>()
            .GetDefaultMenuItems(new[] { pod })
            .Select(x => x.Header)
            .ToList();

        var expectedCustomHeaders = cluster
            .GetResourceConfig<V1Pod>()
            .GetCustomMenuItems(new[] { pod })
            .Select(x => x.Header)
            .ToList();

        node.ContextMenuItems.Select(x => x.Header).Take(expectedDefaultHeaders.Count).ShouldBe(expectedDefaultHeaders);
        node.ContextMenuItems.Any(x => x.IsSeparator).ShouldBeTrue();
        foreach (var header in expectedCustomHeaders)
        {
            node.ContextMenuItems.Select(x => x.Header).ShouldContain(header);
        }

        var view = new VisualizationView
        {
            DataContext = viewModel,
        };

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var svg = view.GetVisualDescendants()
            .OfType<Control>()
            .Single(control => control.ContextMenu != null);

        var contextMenu = svg.ContextMenu.ShouldBeOfType<ContextMenu>();
        var clickPoint = svg.TranslatePoint(new Point(svg.Bounds.Width / 2, svg.Bounds.Height / 2), window);
        clickPoint.ShouldNotBeNull();
        window.MouseDown(clickPoint.Value, MouseButton.Right);
        window.MouseUp(clickPoint.Value, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();

        var headers = (contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>)
            ?.Select(x => x.Header)
            .ToList();

        headers.ShouldNotBeNull();
        headers.ShouldBe(node.ContextMenuItems.Select(x => x.Header).ToList());

        window.Content = null;
        window.Close();
    }
}
