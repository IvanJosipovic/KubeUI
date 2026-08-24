using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
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
    public async Task crd_properties_view_initializes_from_generic_resource()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync();
        var config = ActivatorUtilities.CreateInstance<CRDResourceConfig>(services);
        config.Initialize(workspace);
        config.Configure(new V1CustomResourceDefinition
        {
            Spec = new V1CustomResourceDefinitionSpec
            {
                Group = "example.com",
                Scope = "Namespaced",
                Names = new V1CustomResourceDefinitionNames
                {
                    Kind = "Example",
                    Plural = "examples",
                },
                Versions =
                [
                    new V1CustomResourceDefinitionVersion
                    {
                        Name = "v1",
                        Served = true,
                        Storage = true,
                    }
                ]
            }
        });
        workspace.AddResourceConfigForTest(config);
        workspace.Runtime.ModelCatalog.RegisterCustomResourceDefinition(config.Kind);

        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "example.com/v1",
              "kind": "Example",
              "metadata": {
                "name": "example-1",
                "namespace": "default"
              }
            }
            """);
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<GenericKubernetesObject>>();
        viewModel.Initialize(workspace, resource);

        viewModel.Kind.ShouldBe(config.Kind);
        viewModel.ResourceConfig.ShouldBe(config);

        var view = new ResourcePropertiesView<GenericKubernetesObject>
        {
            DataContext = viewModel,
        };
        var window = new Window { Content = view };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

            var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();
            var nameItem = items.Single(item => item.Key == AppResources.ResourcePropertiesView_Name);
            nameItem.Value.ShouldBe("example-1");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task cluster_aware_property_controls_are_initialized_once()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync();

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
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

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
        using var workspace = await Application.Current.CreateClusterAsync();

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
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

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
        using var workspace = await Application.Current.CreateClusterAsync();
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
