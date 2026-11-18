using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Controls.Layout;
using KubeUI.Features.Clusters.Presentation;
using KubeUI.Features.Resources.Contracts;
using KubeUI.Features.Resources.Visualization;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;
using Ursa.Controls;

namespace KubeUI.Features.Resources.Tests.Visualization;

public sealed class VisualizationNamespacePickerTests
{
    [AvaloniaFact]
    public async Task Toolbar_shows_entire_namespace_link_button_when_space_is_available()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var viewModel = new VisualizationViewModel(new ResourceRelationshipBuilder());
        viewModel.Initialize(workspace);
        var view = new VisualizationView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(width: 1200, content: view);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var toolbar = view.GetVisualDescendants().OfType<CompactToolbar>().Single();
        var picker = view.GetVisualDescendants().OfType<NamespaceScopePicker>().Single();
        var link = picker.GetVisualDescendants().OfType<ToggleButton>().Single();
        var trailing = toolbar.Trailing.ShouldNotBeNull();
        var right = link.TranslatePoint(new Point(link.Bounds.Width, 0), trailing);
        link.Bounds.Width.ShouldBeGreaterThan(0);
        right.ShouldNotBeNull();
        right.Value.X.ShouldBeLessThanOrEqualTo(trailing.Bounds.Width);
        link.TranslatePoint(new Point(link.Bounds.Width, 0), toolbar)!.Value.X
            .ShouldBeLessThanOrEqualTo(toolbar.Bounds.Width);

        TestScreenshot.CaptureIfEnabled(window, "VisualizationToolbar_After");

        window.Width = 320;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var scroll = toolbar.Children.OfType<ScrollViewer>().Single();
        scroll.Extent.Width.ShouldBeGreaterThan(scroll.Viewport.Width);
        link.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        scroll.Offset.X.ShouldBeGreaterThan(0);
        link.TranslatePoint(new Point(link.Bounds.Width, 0), scroll)!.Value.X
            .ShouldBeLessThanOrEqualTo(scroll.Bounds.Width);
    }

    [AvaloniaFact]
    public async Task Resource_type_picker_stays_compact_with_many_selected_types()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var viewModel = new VisualizationViewModel(new ResourceRelationshipBuilder());
        viewModel.Initialize(workspace);
        var view = new VisualizationView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var picker = view.GetVisualDescendants().OfType<MultiComboBox>()
            .Single(control => ReferenceEquals(control.ItemsSource, viewModel.ResourceTypes));
        var namespacePicker = view.GetVisualDescendants().OfType<NamespaceScopePicker>().Single()
            .GetVisualDescendants().OfType<MultiComboBox>().Single();
        for (var i = 0; i < 20; i++)
        {
            var type = $"ResourceType{i}";
            viewModel.ResourceTypes.Add(type);
            viewModel.SelectedResourceTypes.Add(type);
        }

        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        picker.SelectedItems!.Count.ShouldBe(20);
        picker.MaxHeight.ShouldBe(namespacePicker.MaxHeight);
        picker.Bounds.Height.ShouldBeLessThanOrEqualTo(namespacePicker.Bounds.Height);
    }

    [AvaloniaFact]
    public async Task Readonly_named_scope_disables_selection_and_link_without_changing_workspace_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var selected = new V1Namespace { Metadata = new V1ObjectMeta { Name = "original" } };
        workspace.SelectedNamespaces.Clear();
        workspace.SelectedNamespaces.Add(selected);
        using var viewModel = new VisualizationViewModel(new ResourceRelationshipBuilder());
        viewModel.InitializeScope(workspace, new VisualizationScope("scoped", "Scoped resources",
            new ResourceAnnotationSelector(new Dictionary<string, string> { ["example.test/scope"] = "scoped" }), IsNamespaceReadOnly: true));
        var view = new VisualizationView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var picker = view.GetVisualDescendants().OfType<NamespaceScopePicker>().Single();
        picker.IsEffectivelyVisible.ShouldBeTrue();
        picker.IsSelectionEnabled.ShouldBeFalse();
        picker.IsLinkEnabled.ShouldBeFalse();
        picker.GetVisualDescendants().OfType<MultiComboBox>().Single().IsEffectivelyEnabled.ShouldBeFalse();
        picker.GetVisualDescendants().OfType<ToggleButton>().Single().IsEffectivelyEnabled.ShouldBeFalse();
        picker.SelectedNamespaces.ShouldBeSameAs(viewModel.SelectedNamespaces);
        viewModel.SelectedNamespaces.Single().Metadata.Name.ShouldBe("scoped");
        workspace.SelectedNamespaces.Single().ShouldBeSameAs(selected);
    }

    [AvaloniaFact]
    public async Task Resource_root_hides_the_whole_picker_and_namespace_scope_restores_it()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var viewModel = new VisualizationViewModel(new ResourceRelationshipBuilder());
        viewModel.Initialize(workspace);
        var view = new VisualizationView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var picker = view.GetVisualDescendants().OfType<NamespaceScopePicker>().Single();
        picker.IsVisible.ShouldBeTrue();

        viewModel.RootResource = new V1Pod { Metadata = new V1ObjectMeta { Name = "root", NamespaceProperty = "default" } };
        Dispatcher.UIThread.RunJobs();
        picker.IsVisible.ShouldBeFalse();

        viewModel.RootResource = null;
        Dispatcher.UIThread.RunJobs();
        picker.IsVisible.ShouldBeTrue();
        picker.IsSelectionEnabled.ShouldBeTrue();
        picker.IsLinkEnabled.ShouldBeTrue();
    }
}
