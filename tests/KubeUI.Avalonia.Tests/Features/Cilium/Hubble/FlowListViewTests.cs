using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium.Hubble;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Ursa.Controls;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class FlowListViewTests
{
    [AvaloniaFact]
    public async Task Populated_flow_list_renders_policy_cell()
    {
        var services = Application.Current.GetTestServices();
        await using var viewModel = services.GetRequiredService<FlowListViewModel>();
        viewModel.OnFlowsReceived(
        [
            new FlowViewModel
            {
                Uuid = "flow-1",
                Timestamp = DateTime.UtcNow,
                Verdict = FlowVerdict.Forwarded,
                MatchedPolicyName = "allow-web"
            }
        ]);
        await TestWait.UntilAsync(
            () => viewModel.FlowCount == 1 && viewModel.TableSource.Items.Cast<FlowViewModel>().Any(),
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        var view = services.GetRequiredService<FlowListView>();
        view.DataContext = viewModel;

        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants()
            .OfType<TextBlock>()
            .ShouldContain(textBlock => textBlock.Text == "allow-web");
    }

    [AvaloniaFact]
    public async Task Toolbar_shows_namespace_and_search_filters_only()
    {
        var services = Application.Current.GetTestServices();
        await using var viewModel = services.GetRequiredService<FlowListViewModel>();
        var view = services.GetRequiredService<FlowListView>();
        view.DataContext = viewModel;

        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var toolbar = view.GetVisualDescendants()
            .OfType<Grid>()
            .Single(grid => grid.Classes.Contains("toolbar"));
        var namespaceSelector = toolbar.GetVisualDescendants().OfType<MultiComboBox>().Single();
        namespaceSelector.PlaceholderText.ShouldBe(Assets.Resources.ResourceListView_SelectNamespace);

        var searchBox = toolbar.GetVisualDescendants()
            .OfType<TextBox>()
            .Single(textBox => textBox.PlaceholderText == Assets.Resources.FlowListView_SearchWatermark);
        var filterControls = (StackPanel)namespaceSelector.Parent!;
        filterControls.Children.IndexOf(searchBox).ShouldBeLessThan(filterControls.Children.IndexOf(namespaceSelector));

        var status = toolbar.Children.OfType<TextBlock>().Single();
        Grid.GetColumn(status).ShouldBe(2);

        var linkToggle = toolbar.GetVisualDescendants()
            .OfType<ToggleButton>()
            .Single(toggle => Equals(ToolTip.GetTip(toggle), Assets.Resources.ResourceListView_NamespaceLink));
        linkToggle.IsChecked.ShouldBe(true);

        toolbar.GetVisualDescendants().OfType<ComboBox>().ShouldBeEmpty();

        toolbar.GetVisualDescendants().OfType<global::Avalonia.Controls.NumericUpDown>().ShouldBeEmpty();
        toolbar.GetVisualDescendants().OfType<ToggleButton>().ShouldHaveSingleItem();
        toolbar.GetVisualDescendants()
            .OfType<Button>()
            .ShouldNotContain(button => Equals(ToolTip.GetTip(button), Assets.Resources.FlowListView_ButtonResetFilter));
    }
}
