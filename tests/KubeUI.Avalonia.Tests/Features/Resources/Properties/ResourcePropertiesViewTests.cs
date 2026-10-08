using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewTests
{
    [AvaloniaFact]
    public async Task metrics_resource_views_refresh_values_and_keep_metrics_instances()
    {
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Core.v1.Namespace.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1Namespace
                {
                    Metadata = new V1ObjectMeta { Name = "namespace-1" },
                    Status = new V1NamespaceStatus { Phase = "Pending" },
                }),
            },
            new V1Namespace
            {
                Metadata = new V1ObjectMeta { Name = "namespace-1" },
                Status = new V1NamespaceStatus { Phase = "Active" },
            },
            (AppResources.Shared_Phase!, "Active"));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Core.v1.Node.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1Node
                {
                    Metadata = new V1ObjectMeta { Name = "node-1" },
                    Status = new V1NodeStatus { NodeInfo = new V1NodeSystemInfo { OperatingSystem = "old-os" } },
                }),
            },
            new V1Node
            {
                Metadata = new V1ObjectMeta { Name = "node-1" },
                Status = new V1NodeStatus { NodeInfo = new V1NodeSystemInfo { OperatingSystem = "new-os" } },
            },
            (AppResources.NodePropertiesView_Operating_System!, "new-os"));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Storage.v1.PersistentVolumeClaim.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1PersistentVolumeClaim
                {
                    Metadata = new V1ObjectMeta { Name = "claim-1", NamespaceProperty = "default" },
                    Status = new V1PersistentVolumeClaimStatus { Phase = "Pending" },
                }),
            },
            new V1PersistentVolumeClaim
            {
                Metadata = new V1ObjectMeta { Name = "claim-1", NamespaceProperty = "default" },
                Status = new V1PersistentVolumeClaimStatus { Phase = "Bound" },
            },
            (AppResources.Shared_Phase!, "Bound"));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Network.v1.Ingress.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1Ingress
                {
                    Metadata = new V1ObjectMeta { Name = "ingress-1", NamespaceProperty = "default" },
                    Spec = new V1IngressSpec { IngressClassName = "old-class" },
                }),
            },
            new V1Ingress
            {
                Metadata = new V1ObjectMeta { Name = "ingress-1", NamespaceProperty = "default" },
                Spec = new V1IngressSpec { IngressClassName = "new-class" },
            },
            (AppResources.IngressPropertiesView_Ingress_Class!, "new-class"));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Workloads.v1.Job.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1Job
                {
                    Metadata = new V1ObjectMeta { Name = "job-1", NamespaceProperty = "default" },
                    Spec = new V1JobSpec { Completions = 1 },
                }),
            },
            new V1Job
            {
                Metadata = new V1ObjectMeta { Name = "job-1", NamespaceProperty = "default" },
                Spec = new V1JobSpec
                {
                    Completions = 4,
                    BackoffLimit = 3,
                },
            },
            (AppResources.JobPropertiesView_Backoff_Limit!, 3));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Workloads.v1.ReplicaSet.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1ReplicaSet
                {
                    Metadata = new V1ObjectMeta { Name = "replicaset-1", NamespaceProperty = "default" },
                    Spec = new V1ReplicaSetSpec { Replicas = 2 },
                }),
            },
            new V1ReplicaSet
            {
                Metadata = new V1ObjectMeta { Name = "replicaset-1", NamespaceProperty = "default" },
                Spec = new V1ReplicaSetSpec { Replicas = 4 },
            },
            (AppResources.Shared_Desired_Replicas!, 4));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Workloads.v1.Deployment.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1Deployment
                {
                    Metadata = new V1ObjectMeta { Name = "deployment-1", NamespaceProperty = "default" },
                    Spec = new V1DeploymentSpec { Replicas = 1 },
                }),
            },
            new V1Deployment
            {
                Metadata = new V1ObjectMeta { Name = "deployment-1", NamespaceProperty = "default" },
                Spec = new V1DeploymentSpec
                {
                    Replicas = 4,
                },
            },
            (AppResources.Shared_Replicas!, 4));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Workloads.v1.DaemonSet.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1DaemonSet
                {
                    Metadata = new V1ObjectMeta { Name = "daemonset-1", NamespaceProperty = "default" },
                    Status = new V1DaemonSetStatus { NumberReady = 1 },
                }),
            },
            new V1DaemonSet
            {
                Metadata = new V1ObjectMeta { Name = "daemonset-1", NamespaceProperty = "default" },
                Status = new V1DaemonSetStatus { NumberReady = 3 },
            },
            (AppResources.DaemonSetPropertiesView_Ready!, 3));
        await AssertMetricsViewRefreshesInPlace(
            new Avalonia.Resources.Workloads.v1.StatefulSet.PropertiesView
            {
                DataContext = CreatePropertiesViewModel(new V1StatefulSet
                {
                    Metadata = new V1ObjectMeta { Name = "statefulset-1", NamespaceProperty = "default" },
                    Spec = new V1StatefulSetSpec { ServiceName = "old-service" },
                }),
            },
            new V1StatefulSet
            {
                Metadata = new V1ObjectMeta { Name = "statefulset-1", NamespaceProperty = "default" },
                Spec = new V1StatefulSetSpec { ServiceName = "new-service" },
            },
            (AppResources.StatefulSetPropertiesView_Service_Name!, "new-service"));
    }

    [AvaloniaFact]
    public async Task namespaced_resource_shows_namespace_property_item()
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
            DataContext = viewModel
        };

        var window = new Window
        {
            Content = view
        };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();

            var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

            items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task resource_properties_view_renders_leaf_actions_and_submenu_flyouts()
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
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "example/app:1",
                    }
                ]
            }
        });

        var view = new ResourcePropertiesView<V1Pod>
        {
            DataContext = viewModel
        };

        var window = new Window
        {
            Content = view
        };

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var buttons = view.FindControl<StackPanel>("PART_Actions")!.Children.OfType<Button>().ToList();

        viewModel.Actions.Single(action => action.Title == "View").ShowInPropertiesView.ShouldBeFalse();
        buttons.Any(button => Equals(ToolTip.GetTip(button), "View")).ShouldBeFalse();
        buttons.Any(button => button.Command != null && button.Flyout == null).ShouldBeTrue();
        var submenus = buttons.Select(button => button.Flyout).OfType<MenuFlyout>().ToList();
        submenus.Count.ShouldBeGreaterThan(1);
        var submenu = submenus[0];
        var submenuItems = submenu.Items.OfType<MenuItem>().ToList();
        submenuItems.ShouldNotBeEmpty();
        submenuItems.Any(item => item.Items.OfType<MenuItem>().Any()).ShouldBeTrue();

        window.Close();
    }

    [AvaloniaFact]
    public async Task cluster_scoped_resource_hides_namespace_property_item()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync();
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1Node>>();
        viewModel.Initialize(workspace, new V1Node
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView<V1Node>
        {
            DataContext = viewModel
        };

        var window = new Window
        {
            Content = view
        };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();

            var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

            items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task pod_properties_view_shows_ephemeral_containers_section_when_present()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                EphemeralContainers =
                [
                    new V1EphemeralContainer
                    {
                        Name = "debug",
                        Image = "example.com/debug:1",
                    }
                ],
            },
        };

        var view = new Avalonia.Resources.Workloads.v1.Pod.PropertiesView
        {
            DataContext = CreatePropertiesViewModel(pod),
        };

        var window = new Window
        {
            Content = view,
        };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            await TestApplicationExtensions.WaitForUiAsync();

            var section = view.GetVisualDescendants()
                .OfType<ExpandableSection>()
                .Single(x => Equals(x.Header, AppResources.PodPropertiesView_EphemeralContainers));

            section.IsVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task resource_updates_raise_object_changed_even_for_same_instance()
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.SeedResource<V1Pod>(true);
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1Pod>>();
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        };

        viewModel.Initialize(workspace, pod);

        await workspace.Runtime.AddOrUpdateResource(pod);
        await TestApplicationExtensions.WaitForUiAsync();
        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResource<V1Pod>("default", "pod-1") is not null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        pod.Metadata.Labels = new Dictionary<string, string>
        {
            ["updated"] = "true",
        };

        await workspace.Runtime.AddOrUpdateResource(pod);
        await TestApplicationExtensions.WaitForUiAsync();

        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResource<V1Pod>("default", "pod-1")?.Metadata?.Labels?.TryGetValue("updated", out var value) == true
                && value == "true",
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        workspace.Runtime.GetResource<V1Pod>("default", "pod-1")!.Metadata.Labels.ShouldContainKeyAndValue("updated", "true");
    }

    [AvaloniaFact]
    public async Task same_pod_updates_refresh_property_values_without_recreating_metrics_controls()
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
                Labels = new Dictionary<string, string> { ["version"] = "old" },
            },
            Spec = new V1PodSpec
            {
                NodeName = "node-old",
                Containers = [new V1Container { Name = "app", Image = "example/app:old" }],
            },
            Status = new V1PodStatus { Phase = "Pending", PodIP = "10.0.0.1" },
        });
        var view = new ResourcePropertiesView<V1Pod> { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestWait.UntilAsync(
            () => view.GetVisualDescendants().OfType<MetricsControl>().Count() >= 2,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var originalPropertiesView = view.GetVisualDescendants()
            .OfType<Avalonia.Resources.Workloads.v1.Pod.PropertiesView>()
            .Single();
        var originalNameItem = view.FindControl<StackPanel>("PART_Items")!.Children
            .OfType<PropertyItem>()
            .Single(item => item.Key == AppResources.ResourcePropertiesView_Name);
        var originalMetricsControls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
        var updatedPod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
                Labels = new Dictionary<string, string> { ["version"] = "new" },
            },
            Spec = new V1PodSpec
            {
                NodeName = "node-new",
                Containers = [new V1Container { Name = "app", Image = "example/app:new" }],
            },
            Status = new V1PodStatus { Phase = "Running", PodIP = "10.0.0.2" },
        };

        viewModel.Object = updatedPod;

        await TestWait.UntilAsync(
            () =>
            {
                var currentMetricsControls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
                return view.GetVisualDescendants().OfType<PropertyItem>()
                        .Any(item => item.Key == AppResources.PodPropertiesView_Status && Equals(item.Value, "Running"))
                    && currentMetricsControls.Length == originalMetricsControls.Length
                    && currentMetricsControls.All(metricsControl => ReferenceEquals(metricsControl.DataContext, updatedPod));
            },
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () =>
            {
                var currentMetricsControls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
                return originalMetricsControls.All(originalMetricsControl => currentMetricsControls.Contains(originalMetricsControl));
            },
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var updatedMetricsControls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
        view.GetVisualDescendants()
            .OfType<Avalonia.Resources.Workloads.v1.Pod.PropertiesView>()
            .Single()
            .ShouldBeSameAs(originalPropertiesView);
        view.FindControl<StackPanel>("PART_Items")!.Children
            .OfType<PropertyItem>()
            .Single(item => item.Key == AppResources.ResourcePropertiesView_Name)
            .ShouldBeSameAs(originalNameItem);
        updatedMetricsControls.Length.ShouldBe(originalMetricsControls.Length);
        var originalPodMetricsControl = originalMetricsControls.Single(metricsControl => metricsControl.Container == null);
        var originalContainerMetricsControl = originalMetricsControls.Single(metricsControl => metricsControl.Container != null);
        updatedMetricsControls.ShouldContain(metricsControl => ReferenceEquals(metricsControl, originalPodMetricsControl));
        originalPodMetricsControl.DataContext.ShouldBeSameAs(updatedPod);
        var containerMetricsControl = updatedMetricsControls.Single(metricsControl => metricsControl.Container != null);
        updatedMetricsControls.ShouldContain(metricsControl => ReferenceEquals(metricsControl, originalContainerMetricsControl));
        containerMetricsControl.ShouldBeSameAs(originalContainerMetricsControl);
        containerMetricsControl.Pod.ShouldBeSameAs(updatedPod);
        containerMetricsControl.Container.ShouldBeSameAs(updatedPod.Spec.Containers!.Single());

        await TestWait.UntilAsync(
            () => view.GetVisualDescendants().OfType<PropertyItem>()
                .Any(item => item.Key == AppResources.PodPropertiesView_Image && Equals(item.Value, "example/app:new")),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        view.GetVisualDescendants().OfType<PropertyItem>()
            .ShouldContain(item => item.Key == AppResources.PodPropertiesView_Node && Equals(item.Value, "node-new"));
        view.GetVisualDescendants().OfType<PropertyItem>()
            .ShouldContain(item => item.Key == AppResources.PodPropertiesView_PodIp && Equals(item.Value, "10.0.0.2"));
        view.GetVisualDescendants().OfType<PropertyItem>()
            .ShouldContain(item => item.Key == AppResources.PodPropertiesView_Image && Equals(item.Value, "example/app:new"));
        view.GetVisualDescendants().OfType<CollectionItem>()
            .ShouldContain(item => item.Key == AppResources.PodPropertiesView_Labels
                && item.Value.Cast<KeyValuePair<string, string>>().Any(pair => pair.Key == "version" && pair.Value == "new"));
    }

    [AvaloniaFact]
    public async Task different_resource_identity_rebuilds_properties_controls()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync();
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1ReplicaSet>>();
        viewModel.Initialize(workspace, new V1ReplicaSet
        {
            Metadata = new V1ObjectMeta { Name = "replicaset-1", NamespaceProperty = "default" },
            Spec = new V1ReplicaSetSpec { Replicas = 2 },
        });
        var view = new ResourcePropertiesView<V1ReplicaSet> { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestWait.UntilAsync(
            () => view.GetVisualDescendants().OfType<MetricsControl>().Any(),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var originalPropertiesView = view.GetVisualDescendants()
            .OfType<Avalonia.Resources.Workloads.v1.ReplicaSet.PropertiesView>()
            .Single();
        var originalMetricsControl = view.GetVisualDescendants().OfType<MetricsControl>().Single();

        viewModel.Object = new V1ReplicaSet
        {
            Metadata = new V1ObjectMeta { Name = "replicaset-2", NamespaceProperty = "default" },
            Spec = new V1ReplicaSetSpec { Replicas = 5 },
        };

        await TestWait.UntilAsync(
            () =>
            {
                var propertiesViews = view.GetVisualDescendants()
                    .OfType<Avalonia.Resources.Workloads.v1.ReplicaSet.PropertiesView>()
                    .ToArray();
                var metricsControls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
                return view.FindControl<StackPanel>("PART_Items")!.Children
                           .OfType<PropertyItem>()
                           .Any(item => item.Key == AppResources.ResourcePropertiesView_Name && Equals(item.Value, "replicaset-2"))
                    && propertiesViews.Length == 1
                    && !ReferenceEquals(propertiesViews[0], originalPropertiesView)
                    && metricsControls.Length == 1
                    && !ReferenceEquals(metricsControls[0], originalMetricsControl);
            },
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        view.GetVisualDescendants()
            .OfType<Avalonia.Resources.Workloads.v1.ReplicaSet.PropertiesView>()
            .Single()
            .ShouldNotBeSameAs(originalPropertiesView);
        view.GetVisualDescendants().OfType<MetricsControl>().Single().ShouldNotBeSameAs(originalMetricsControl);
    }

    [AvaloniaFact]
    public async Task pod_container_set_changes_update_rows_without_rebuilding_pod_properties_view()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync();
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1Pod>>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "example/app:1" }] },
        });
        var view = new ResourcePropertiesView<V1Pod> { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestWait.UntilAsync(
            () => view.GetVisualDescendants().OfType<MetricsControl>().Count() == 2,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var propertiesView = view.GetVisualDescendants()
            .OfType<Avalonia.Resources.Workloads.v1.Pod.PropertiesView>()
            .Single();
        var podMetricsControl = view.GetVisualDescendants().OfType<MetricsControl>()
            .Single(control => control.Container is null);

        var updatedPod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod-1", NamespaceProperty = "default" },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container { Name = "app", Image = "example/app:2" },
                    new V1Container { Name = "sidecar", Image = "example/sidecar:1" },
                ],
            },
        };
        viewModel.Object = updatedPod;

        await TestWait.UntilAsync(
            () => view.GetVisualDescendants().OfType<MetricsControl>().Count() == 3
                && view.GetVisualDescendants().OfType<PropertyItem>()
                    .Any(item => item.Key == AppResources.PodPropertiesView_Image && Equals(item.Value, "example/sidecar:1")),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        view.GetVisualDescendants()
            .OfType<Avalonia.Resources.Workloads.v1.Pod.PropertiesView>()
            .Single()
            .ShouldBeSameAs(propertiesView);
        view.GetVisualDescendants().OfType<MetricsControl>()
            .ShouldContain(control => ReferenceEquals(control, podMetricsControl));
        view.GetVisualDescendants().OfType<MetricsControl>()
            .Where(control => control.Container is not null)
            .Select(control => control.Container!.Name)
            .ShouldBe(new[] { "app", "sidecar" }, ignoreOrder: true);
        foreach (var metricsControl in view.GetVisualDescendants().OfType<MetricsControl>()
                     .Where(control => control.Container is not null))
        {
            metricsControl.Pod.ShouldBeSameAs(updatedPod);
        }
    }

    [AvaloniaFact]
    public async Task detached_resource_properties_view_does_not_throw_when_view_model_changes()
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

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        window.Content = null;
        window.Close();
        await TestApplicationExtensions.WaitForUiAsync();

        viewModel.Object = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-2",
                NamespaceProperty = "default",
            }
        };

        await TestApplicationExtensions.WaitForUiAsync();
    }

    private static async Task AssertMetricsViewRefreshesInPlace<TResource, TPropertiesView>(
        TPropertiesView view,
        TResource updatedResource,
        params (string Key, object? Value)[] expectedValues)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
        where TPropertiesView : Control
    {
        var viewModel = view.DataContext.ShouldBeOfType<ResourcePropertiesViewModel<TResource>>();

        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        await TestWait.UntilAsync(
            () => view.GetVisualDescendants().OfType<MetricsControl>().Any(),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var sections = view.GetVisualDescendants().OfType<ExpandableSection>().ToArray();
        foreach (var section in sections)
        {
            section.IsExpanded = false;
        }

        Dispatcher.UIThread.RunJobs();
        var metricsControl = view.GetVisualDescendants().OfType<MetricsControl>().Single();
        await Dispatcher.UIThread.InvokeAsync(() => viewModel.Object = updatedResource);
        foreach (var section in sections)
        {
            section.IsExpanded = true;
        }

        await TestWait.UntilAsync(
            () =>
            {
                var propertyItems = view.GetVisualDescendants().OfType<PropertyItem>().ToArray();
                return expectedValues.All(expectedValue => propertyItems.Any(item => item.Key == expectedValue.Key));
            },
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var refreshedPropertyItems = view.GetVisualDescendants().OfType<PropertyItem>().ToArray();
        foreach (var expectedValue in expectedValues)
        {
            refreshedPropertyItems.Single(item => item.Key == expectedValue.Key).Value.ShouldBe(expectedValue.Value);
        }
        view.GetVisualDescendants().OfType<MetricsControl>().Single().ShouldBeSameAs(metricsControl);
    }

    private static ResourcePropertiesViewModel<TResource> CreatePropertiesViewModel<TResource>(TResource resource)
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new() => new()
        {
            Object = resource,
        };
}
