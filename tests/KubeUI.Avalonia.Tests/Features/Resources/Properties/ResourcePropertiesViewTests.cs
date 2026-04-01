using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Views;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void namespaced_resource_shows_namespace_property_item()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();
        var viewModel = new ResourcePropertiesViewModel<V1Pod>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };

        var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void cluster_scoped_resource_hides_namespace_property_item()
    {
        var workspace = new TestCluster().CreateWorkspace();
        workspace.EnsureWorkspaceStateInitializedAsync().GetAwaiter().GetResult();
        var viewModel = new ResourcePropertiesViewModel<V1Node>();
        viewModel.Initialize(workspace, new V1Node
        {
            Metadata = new V1ObjectMeta
            {
                Name = "node-1",
                NamespaceProperty = "default",
            }
        });

        var view = new ResourcePropertiesView
        {
            DataContext = viewModel
        };

        var items = view.FindControl<StackPanel>("PART_Items")!.Children.OfType<PropertyItem>().ToList();

        items.Any(x => x.Key == AppResources.ResourcePropertiesView_Namespace).ShouldBeFalse();
    }
}
