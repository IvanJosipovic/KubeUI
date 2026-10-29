using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium.Hubble;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class FlowListViewModelTests
{
    [AvaloniaFact]
    public async Task Flow_batch_keeps_newest_events_by_timestamp_and_bounds_cache()
    {
        await using var viewModel = new FlowListViewModel(
            NullLogger<FlowListViewModel>.Instance,
            Mock.Of<IHubbleConnectionFactory>());
        viewModel.MaxFlows = 2;

        await Task.Run(() => viewModel.OnFlowsReceived(
        [
            Flow("middle", DateTimeOffset.Parse("2026-10-23T10:00:02Z").UtcDateTime),
            Flow("oldest", DateTimeOffset.Parse("2026-10-23T10:00:01Z").UtcDateTime),
            Flow("newest", DateTimeOffset.Parse("2026-10-23T10:00:03Z").UtcDateTime)
        ]), TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => viewModel.FlowCount == 2
                && viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 2,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        viewModel.FlowCount.ShouldBe(2);
        viewModel.TableSource.Items
            .Cast<FlowViewModel>()
            .Select(static flow => flow.Uuid)
            .ShouldBe(["newest", "middle"]);
    }

    [AvaloniaFact]
    public async Task Active_filter_matches_survive_unmatched_flow_eviction()
    {
        await using var viewModel = new FlowListViewModel(
            NullLogger<FlowListViewModel>.Instance,
            Mock.Of<IHubbleConnectionFactory>());
        viewModel.MaxFlows = 3;
        viewModel.OnFlowsReceived(
        [
            Flow("old-forwarded", DateTime.Parse("2026-10-23T10:00:01Z")),
            Flow("dropped-one", DateTime.Parse("2026-10-23T10:00:02Z"), verdict: FlowVerdict.Dropped),
            Flow("dropped-two", DateTime.Parse("2026-10-23T10:00:03Z"), verdict: FlowVerdict.Dropped)
        ]);

        await TestWait.UntilAsync(
            () => viewModel.FlowCount == 3
                && viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 3,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        viewModel.Filter.ShowOnlyErrors = true;
        await TestWait.UntilAsync(
            () => viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 2,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        viewModel.OnFlowsReceived(
        [
            Flow("new-forwarded-one", DateTime.Parse("2026-10-23T10:00:04Z")),
            Flow("new-forwarded-two", DateTime.Parse("2026-10-23T10:00:05Z")),
            Flow("new-forwarded-three", DateTime.Parse("2026-10-23T10:00:06Z")),
            Flow("dropped-three", DateTime.Parse("2026-10-23T10:00:07Z"), verdict: FlowVerdict.Dropped)
        ]);

        await TestWait.UntilAsync(
            () => viewModel.FlowCount == 3
                && viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 3,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        viewModel.TableSource.Items
            .Cast<FlowViewModel>()
            .Select(static flow => flow.Uuid)
            .ShouldBe(["dropped-one", "dropped-two", "dropped-three"], ignoreOrder: true);
    }

    [AvaloniaFact]
    public void Custom_flow_cell_templates_accept_null_items()
    {
        foreach (var column in FlowListViewModel.CreateColumns())
        {
            if (column.CellTemplate is not null)
            {
                column.CellTemplate.Build(null).ShouldNotBeNull();
            }
        }
    }

    [Fact]
    public void Direction_column_displays_localized_text_without_an_icon_template()
    {
        FlowListViewModel.FormatDirection(FlowDirection.Ingress)
            .ShouldBe(Assets.Resources.FlowDirection_Ingress);
        FlowListViewModel.FormatDirection(FlowDirection.Egress)
            .ShouldBe(Assets.Resources.FlowDirection_Egress);
        FlowListViewModel.FormatDirection(FlowDirection.Unknown)
            .ShouldBe(Assets.Resources.FlowDirection_Unknown);

        FlowListViewModel.CreateColumns()
            .Single(static column => column.Key == "Direction")
            .CellTemplate.ShouldBeNull();
    }

    [Fact]
    public void Verdict_column_displays_localized_text_without_a_badge_template()
    {
        FlowListViewModel.FormatVerdict(FlowVerdict.Forwarded)
            .ShouldBe(Assets.Resources.FlowVerdict_Allowed);
        FlowListViewModel.FormatVerdict(FlowVerdict.Dropped)
            .ShouldBe(Assets.Resources.FlowVerdict_Dropped);
        FlowListViewModel.FormatVerdict(FlowVerdict.Error)
            .ShouldBe(Assets.Resources.FlowVerdict_Error);
        FlowListViewModel.FormatVerdict(FlowVerdict.Unknown)
            .ShouldBe(Assets.Resources.FlowVerdict_Unknown);

        FlowListViewModel.CreateColumns()
            .Single(static column => column.Key == "Verdict")
            .CellTemplate.ShouldBeNull();
    }

    [Fact]
    public void Source_and_destination_columns_show_pod_names_without_endpoint_templates()
    {
        var columns = FlowListViewModel.CreateColumns();

        columns.Single(static column => column.Key == "Source").CellTemplate.ShouldBeNull();
        columns.Single(static column => column.Key == "Destination").CellTemplate.ShouldBeNull();
    }

    [Fact]
    public void Endpoint_columns_use_equal_star_widths_and_basic_string_columns_use_auto_width()
    {
        var columns = FlowListViewModel.CreateColumns();

        var source = columns.Single(static column => column.Key == "Source");
        var destination = columns.Single(static column => column.Key == "Destination");

        source.WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
        source.Width.ShouldBe(1.0);
        destination.WidthMode.ShouldBe(DynamicTableViewWidthMode.Star);
        destination.Width.ShouldBe(source.Width);
        columns.Single(static column => column.Key == "Protocol").WidthMode.ShouldBe(DynamicTableViewWidthMode.Auto);
        columns.Single(static column => column.Key == "Latency").WidthMode.ShouldBe(DynamicTableViewWidthMode.Auto);
        columns.Single(static column => column.Key == "PolicyName").WidthMode.ShouldBe(DynamicTableViewWidthMode.Auto);
    }

    [AvaloniaFact]
    public async Task Namespace_selection_filters_flows_touching_selected_namespaces()
    {
        await using var viewModel = new FlowListViewModel(
            NullLogger<FlowListViewModel>.Instance,
            Mock.Of<IHubbleConnectionFactory>());
        viewModel.OnFlowsReceived(
        [
            Flow("source-match", DateTime.UtcNow, sourceNamespace: "team-a"),
            Flow("destination-match", DateTime.UtcNow, destinationNamespace: "team-a"),
            Flow("no-match", DateTime.UtcNow, sourceNamespace: "team-b", destinationNamespace: "team-c")
        ]);

        await TestWait.UntilAsync(
            () => viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 3,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        viewModel.SelectedNamespaces.Add(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-a" }
        });

        await TestWait.UntilAsync(
            () => viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 2,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        viewModel.TableSource.Items
            .Cast<FlowViewModel>()
            .Select(static flow => flow.Uuid)
            .ShouldBe(["source-match", "destination-match"], ignoreOrder: true);

        viewModel.SelectedNamespaces.Clear();
        await TestWait.UntilAsync(
            () => viewModel.TableSource.Items.Cast<FlowViewModel>().Count() == 3,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task Namespace_selection_starts_linked_and_can_be_unlinked_locally()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var clusterNamespace = new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "team-a" }
        };
        workspace.SelectedNamespaces.Add(clusterNamespace);
        await using var viewModel = new FlowListViewModel(
            NullLogger<FlowListViewModel>.Instance,
            Mock.Of<IHubbleConnectionFactory>())
        {
            Cluster = workspace
        };

        viewModel.IsNamespaceSelectionLinked.ShouldBeTrue();
        viewModel.SelectedNamespaces.ShouldBeSameAs(workspace.SelectedNamespaces);

        viewModel.IsNamespaceSelectionLinked = false;

        viewModel.SelectedNamespaces.ShouldNotBeSameAs(workspace.SelectedNamespaces);
        viewModel.SelectedNamespaces.ShouldContain(clusterNamespace);
        viewModel.SelectedNamespaces.Clear();
        workspace.SelectedNamespaces.ShouldContain(clusterNamespace);

        viewModel.IsNamespaceSelectionLinked = true;

        viewModel.SelectedNamespaces.ShouldBeSameAs(workspace.SelectedNamespaces);
        viewModel.SelectedNamespaces.ShouldContain(clusterNamespace);
    }

    private static FlowViewModel Flow(
        string uuid,
        DateTime timestamp,
        string sourceNamespace = "",
        string destinationNamespace = "",
        FlowVerdict verdict = FlowVerdict.Forwarded)
        => new()
        {
            Uuid = uuid,
            Timestamp = timestamp,
            Verdict = verdict,
            SourceNamespace = sourceNamespace,
            DestinationNamespace = destinationNamespace
        };
}
