using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task namespaced_resource_shows_namespace_property_item()
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
            DataContext = viewModel
        };

        var window = new Window
        {
            Content = view
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task resource_properties_view_renders_leaf_actions_and_submenu_flyouts()
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
        Dispatcher.UIThread.RunJobs();

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
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        var workspace = scope.Workspace;
        await workspace.Connect();
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
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

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeFalse();
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

        var view = new KubeUI.Avalonia.Resources.Workloads.v1.Pod.PropertiesView
        {
            DataContext = pod,
        };

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        ExpandableSection section = view.GetVisualDescendants()
            .OfType<ExpandableSection>()
            .Single(x => Equals(x.Header, AppResources.PodPropertiesView_EphemeralContainers));

        section.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task resource_updates_raise_object_changed_even_for_same_instance()
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        var workspace = scope.Workspace;
        await workspace.Connect();
        await workspace.Runtime.SeedResource<V1Pod>(true);
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
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
        Dispatcher.UIThread.RunJobs();
        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResource<V1Pod>("default", "pod-1") is not null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        pod.Metadata.Labels = new Dictionary<string, string>
        {
            ["updated"] = "true",
        };

        await workspace.Runtime.AddOrUpdateResource(pod);
        Dispatcher.UIThread.RunJobs();

        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResource<V1Pod>("default", "pod-1")?.Metadata?.Labels?.TryGetValue("updated", out string? value) == true
                && value == "true",
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        workspace.Runtime.GetResource<V1Pod>("default", "pod-1")!.Metadata.Labels.ShouldContainKeyAndValue("updated", "true");
    }

    [AvaloniaFact]
    public async Task detached_resource_properties_view_does_not_throw_when_view_model_changes()
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

        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();

        viewModel.Object = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-2",
                NamespaceProperty = "default",
            }
        };

        Dispatcher.UIThread.RunJobs();
    }
}
