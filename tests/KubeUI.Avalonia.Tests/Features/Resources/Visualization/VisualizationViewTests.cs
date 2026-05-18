using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
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
}
