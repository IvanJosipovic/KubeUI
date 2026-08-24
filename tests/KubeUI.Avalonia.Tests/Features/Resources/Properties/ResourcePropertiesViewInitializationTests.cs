using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewInitializationTests
{
    [AvaloniaFact]
    public async Task cluster_aware_property_controls_are_initialized_once()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync();

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

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            await TestApplicationExtensions.WaitForUiAsync();

            trackingConfig.TrackingControl.InitializeCount.ShouldBe(1);
            view.FindControl<StackPanel>("PART_Items")!.Children.ShouldContain(trackingConfig.TrackingControl);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task properties_view_populates_on_first_attach()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync();

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

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            await TestApplicationExtensions.WaitForUiAsync();

            var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

            items.ShouldNotBeEmpty();
            items.Any(x => x.Key == AppResources.ResourcePropertiesView_Name).ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task viewing_another_resource_replaces_existing_properties_view()
    {
        var services = Application.Current.GetTestServices();
        var factory = services.GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var rightDock = factory.GetDockable<IToolDock>("RightDock")!;
        var workspace = await Application.Current.CreateClusterAsync();
        var config = (ResourceConfigBase<V1Pod>)workspace.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        var podA = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod-a", NamespaceProperty = "default" },
        };
        var podB = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod-b", NamespaceProperty = "default" },
        };

        config.View(new[] { podA });
        var firstView = rightDock.VisibleDockables!.Single()
            .ShouldBeOfType<ResourcePropertiesViewModel<V1Pod>>();

        config.View(new[] { podB });

        rightDock.VisibleDockables.ShouldHaveSingleItem();
        var secondView = rightDock.VisibleDockables.Single()
            .ShouldBeOfType<ResourcePropertiesViewModel<V1Pod>>();
        secondView.ShouldNotBeSameAs(firstView);
        secondView.Object.ShouldBeSameAs(podB);
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
