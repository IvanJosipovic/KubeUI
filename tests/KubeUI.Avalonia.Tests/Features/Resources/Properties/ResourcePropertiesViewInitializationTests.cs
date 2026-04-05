using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Views;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewInitializationTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task cluster_aware_property_controls_are_initialized_once()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var trackingConfig = new TrackingResourceConfig(services);
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1Pod>>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });
        viewModel.ResourceConfig = trackingConfig;

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel,
        };

        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        trackingConfig.TrackingControl.InitializeCount.ShouldBe(1);
        view.FindControl<StackPanel>("PART_Items")!.Children.ShouldContain(trackingConfig.TrackingControl);
    }

    private sealed class TrackingResourceConfig : ResourceConfigBase<V1Pod>
    {
        public TrackingResourceConfig(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public TrackingClusterControl TrackingControl { get; } = new();

        public override Control[] Properties(V1Pod resource)
        {
            return [TrackingControl];
        }
    }

    private sealed class TrackingClusterControl : UserControl, IInitializeCluster
    {
        public int InitializeCount { get; private set; }

        public TrackingClusterControl()
        {
            Content = new TextBlock { Text = "tracking" };
        }

        public void Initialize(ClusterWorkspaceViewModel cluster)
        {
            InitializeCount++;
        }
    }
}
