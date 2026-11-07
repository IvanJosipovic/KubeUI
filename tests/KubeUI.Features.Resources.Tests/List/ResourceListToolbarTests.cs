using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using k8s.Models;
using Shouldly;
using Ursa.Controls;

namespace KubeUI.Features.Resources.Tests.List;

public sealed class ResourceListToolbarTests
{
    [AvaloniaFact]
    public async Task Header_retains_localized_count_new_action_and_two_way_search()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate([Pod("api"), Pod("worker")]);
        await WaitForAsync(() => viewModel.ItemCount == 2);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = viewModel;
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => HasCount(view, 2));

        var add = view.GetVisualDescendants().OfType<Button>()
            .Single(button => Equals(ToolTip.GetTip(button), ResourceL10n.Instance.ResourceListView_NewResource.CurrentValue()));
        add.Command.ShouldBeSameAs(viewModel.ResourceConfig.NewResourceCommand);
        add.IsVisible.ShouldBe(viewModel.ResourceConfig.ShowNewResource);
        var search = view.GetVisualDescendants().OfType<TextBox>()
            .Single(text => text.PlaceholderText == ResourceL10n.Instance.ResourceListView_SearchWatermark.CurrentValue());
        search.IsEffectivelyVisible.ShouldBeTrue();
        search.Text = "api";
        await WaitForAsync(() => viewModel.SearchQuery == "api" && viewModel.ItemCount == 1 && HasCount(view, 1));
        viewModel.TableSource.Items.Cast<V1Pod>().Single().Metadata.Name.ShouldBe("api");

        viewModel.SearchQuery = string.Empty;
        await WaitForAsync(() => string.IsNullOrEmpty(search.Text) && viewModel.ItemCount == 2 && HasCount(view, 2));
    }

    [AvaloniaFact]
    public async Task Header_namespace_controls_keep_selection_and_link_bindings()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "team-a" } });
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = viewModel;
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => view.GetVisualDescendants().OfType<MultiComboBox>().Any());

        var namespaces = view.GetVisualDescendants().OfType<MultiComboBox>().Single();
        var link = view.GetVisualDescendants().OfType<ToggleButton>()
            .Single(button => Equals(ToolTip.GetTip(button), "Linked namespace selection"));
        namespaces.IsEffectivelyVisible.ShouldBeTrue();
        namespaces.PlaceholderText.ShouldBe("Filter Namespace(s)");
        namespaces.ItemsSource.ShouldBeSameAs(cluster.Runtime.Namespaces);
        namespaces.SelectedItems.ShouldBeSameAs(viewModel.SelectedNamespaces);
        link.IsEffectivelyVisible.ShouldBeTrue();
        link.IsChecked.ShouldBe(true);

        link.IsChecked = false;
        await WaitForAsync(() => !viewModel.IsNamespaceSelectionLinked &&
            ReferenceEquals(namespaces.SelectedItems, viewModel.SelectedNamespaces));
        viewModel.SelectedNamespaces.ShouldNotBeSameAs(cluster.SelectedNamespaces);
        viewModel.SelectedNamespaces.Single().Metadata.Name.ShouldBe("team-a");

        viewModel.IsNamespaceSelectionLinked = true;
        await WaitForAsync(() => link.IsChecked == true &&
            ReferenceEquals(namespaces.SelectedItems, cluster.SelectedNamespaces));
    }

    private static bool HasCount(Control view, int count)
        => view.GetVisualDescendants().OfType<Label>()
            .Any(label => Equals(label.Content, string.Format(ResourceL10n.Instance.ResourceListView_ItemsFormat.CurrentValue()!, count)));

    private static V1Pod Pod(string name)
        => new()
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = "team-a",
                CreationTimestamp = DateTime.UtcNow
            }
        };

    private static Task WaitForAsync(Func<bool> condition)
        => TestWait.UntilAsync(condition, 5000, TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
}
