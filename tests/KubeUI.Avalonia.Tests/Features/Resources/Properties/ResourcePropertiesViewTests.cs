using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Views;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task namespaced_resource_shows_namespace_property_item()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();
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

        var window = new Window
        {
            Content = new ResourcePropertiesView
            {
                DataContext = viewModel
            }
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var items = window.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task cluster_scoped_resource_hides_namespace_property_item()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();
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

        var window = new Window
        {
            Content = new ResourcePropertiesView
            {
                DataContext = viewModel
            }
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var items = window.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task resource_updates_raise_object_changed_even_for_same_instance()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();
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

        var changeCount = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ResourcePropertiesViewModel<V1Pod>.Object))
            {
                changeCount++;
            }
        };

        await workspace.AddOrUpdateResource(pod);
        Dispatcher.UIThread.RunJobs();
        changeCount = 0;

        pod.Metadata.Labels = new Dictionary<string, string>
        {
            ["updated"] = "true",
        };

        await workspace.AddOrUpdateResource(pod);
        Dispatcher.UIThread.RunJobs();

        changeCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task detached_resource_properties_view_does_not_throw_when_view_model_changes()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();
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

        var window = new Window
        {
            Content = new ResourcePropertiesView
            {
                DataContext = viewModel,
            }
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
