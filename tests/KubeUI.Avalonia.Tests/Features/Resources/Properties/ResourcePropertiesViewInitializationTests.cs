using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewInitializationTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task cluster_aware_property_controls_are_initialized_once()
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        var workspace = scope.Workspace;
        await workspace.Connect();

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var trackingConfig = new TrackingResourceConfig(services);
        trackingConfig.Initialize(workspace);
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

        var view = new ResourcePropertiesView<V1Pod>
        {
            DataContext = viewModel,
        };

        var window = new Window
        {
            Content = view
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        trackingConfig.TrackingControl.InitializeCount.ShouldBe(1);
        view.FindControl<StackPanel>("PART_Items")!.Children.ShouldContain(trackingConfig.TrackingControl);
    }

    [AvaloniaFact]
    public async Task properties_view_populates_on_first_attach()
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        var workspace = scope.Workspace;
        await workspace.Connect();

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1Pod>>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView<V1Pod>
        {
            DataContext = viewModel,
        };

        var window = new Window
        {
            Content = view
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.ShouldNotBeEmpty();
        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Name).ShouldBeTrue();
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

        public ClusterWorkspace? Cluster { get; private set; }

        public TrackingClusterControl()
        {
            Content = new TextBlock { Text = "tracking" };
        }

        public void Initialize(ClusterWorkspace cluster)
        {
            Cluster = cluster;
            InitializeCount++;
        }
    }
}
