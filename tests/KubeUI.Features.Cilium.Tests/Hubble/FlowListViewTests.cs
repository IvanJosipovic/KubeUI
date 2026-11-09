using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.App.Tests.Infra;
using KubeUI.Controls.Layout;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Clusters.Presentation;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Ursa.Controls;

namespace KubeUI.Features.Cilium.Tests.Hubble;

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

        var toolbar = view.GetVisualDescendants().OfType<CompactToolbar>().Single();
        var namespaceSelector = toolbar.GetVisualDescendants().OfType<MultiComboBox>().Single();
        namespaceSelector.PlaceholderText.ShouldBe("Filter Namespace(s)");

        var searchBox = toolbar.GetVisualDescendants()
            .OfType<TextBox>()
            .Single(textBox => textBox.PlaceholderText == CiliumLanguageManager.Instance.FlowListView_SearchWatermark.CurrentValue());
        var picker = toolbar.GetVisualDescendants().OfType<NamespaceScopePicker>().Single();
        var filterControls = (StackPanel)picker.Parent!;
        filterControls.Children.IndexOf(searchBox).ShouldBeLessThan(filterControls.Children.IndexOf(picker));
        picker.PickerWidth.ShouldBe(190);

        toolbar.Middle!.GetVisualDescendants().OfType<TextBlock>()
            .ShouldContain(textBlock => textBlock.Text == viewModel.ConnectionStatus);

        var linkToggle = toolbar.GetVisualDescendants()
            .OfType<ToggleButton>()
            .Single(toggle => Equals(ToolTip.GetTip(toggle), "Linked namespace selection"));
        linkToggle.IsChecked.ShouldBe(true);

        toolbar.GetVisualDescendants().OfType<ComboBox>().ShouldBeEmpty();

        toolbar.GetVisualDescendants().OfType<Avalonia.Controls.NumericUpDown>().ShouldBeEmpty();
        toolbar.GetVisualDescendants().OfType<ToggleButton>().ShouldHaveSingleItem();
        toolbar.GetVisualDescendants()
            .OfType<Button>()
            .ShouldNotContain(button => Equals(ToolTip.GetTip(button), CiliumLanguageManager.Instance.FlowListView_ButtonResetFilter.CurrentValue()));
    }
}
