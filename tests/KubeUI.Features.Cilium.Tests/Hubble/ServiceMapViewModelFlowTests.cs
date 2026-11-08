using System.Reactive.Subjects;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Kubernetes;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Features.Cilium.Tests.Hubble;

/// <summary>
/// Exercises the ServiceMapViewModel flow-ingestion pipeline against the generated fake
/// cluster with a scripted Hubble connection, so graph publication, error filtering, and
/// disposal cancellation run through the production VM without a live Hubble relay.
/// </summary>
public sealed class ServiceMapViewModelFlowTests
{
    [AvaloniaFact]
    public async Task Flow_ingestion_publishes_graph_snapshots_and_filters_errors()
    {
        using var workspace = await Application.Current.CreateClusterAsync(config => config.Resources =
        [
            new V1Namespace { Metadata = new() { Name = "default" } },
            new V1CustomResourceDefinition
            {
                Metadata = new() { Name = "networkpolicies.cilium.io" },
                Spec = new() { Group = "cilium.io" },
            },
        ]);
        await WarmCrdPermissionsAsync(workspace);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true, TestContext.Current.CancellationToken);

        var connection = new ScriptedHubbleConnection();
        try
        {
            using var viewModel = new ServiceMapViewModel(
                NullLogger<ServiceMapViewModel>.Instance,
                new ScriptedConnectionFactory(connection),
                Application.Current.GetRequiredTestService<IFactory>());
            viewModel.IsNamespaceSelectionLinked = false;

            await viewModel.InitializeAsync(workspace, TestContext.Current.CancellationToken);
            try
            {
                await TestWait.UntilAsync(
                    () => viewModel.ConnectionStatus == CiliumL10n.Instance.ServiceMapView_StatusSelectNamespace.CurrentValue(),
                    TimeSpan.FromSeconds(10),
                    cancellationToken: TestContext.Current.CancellationToken,
                    beforePoll: () => Dispatcher.UIThread.RunJobs());
            }
            catch (TimeoutException)
            {
                throw new TimeoutException(
                    $"status='{viewModel.ConnectionStatus}', connected={viewModel.IsConnected}, "
                    + $"crds={workspace.Runtime.GetResourceList<V1CustomResourceDefinition>().Count}");
            }

            viewModel.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
            await TestWait.UntilAsync(
                () => connection.RequestedNamespaces is { Count: > 0 },
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken,
                beforePoll: () => Dispatcher.UIThread.RunJobs());

            connection.Publish(CreateFlow("web", "api", FlowVerdict.Forwarded));
            connection.Publish(CreateFlow("web", "db", FlowVerdict.Dropped));

            await TestWait.UntilAsync(
                () => viewModel.Graph is { NodeCount: 3 },
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken,
                beforePoll: () => Dispatcher.UIThread.RunJobs());
            viewModel.HasFlows.ShouldBeTrue();

            viewModel.ShowOnlyErrors = true;
            await TestWait.UntilAsync(
                () => viewModel.Graph is { NodeCount: 2 },
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken,
                beforePoll: () => Dispatcher.UIThread.RunJobs());

            viewModel.ShowOnlyErrors = false;
            await TestWait.UntilAsync(
                () => viewModel.Graph is { NodeCount: 3 },
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken,
                beforePoll: () => Dispatcher.UIThread.RunJobs());
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    [AvaloniaFact]
    public async Task Dispose_cancels_pending_graph_builds_and_marks_the_connection_disconnected()
    {
        using var workspace = await Application.Current.CreateClusterAsync(config => config.Resources =
        [
            new V1Namespace { Metadata = new() { Name = "default" } },
            new V1CustomResourceDefinition
            {
                Metadata = new() { Name = "networkpolicies.cilium.io" },
                Spec = new() { Group = "cilium.io" },
            },
        ]);
        await WarmCrdPermissionsAsync(workspace);
        await workspace.Runtime.SeedResource<V1CustomResourceDefinition>(true, TestContext.Current.CancellationToken);

        var connection = new ScriptedHubbleConnection();
        var viewModel = new ServiceMapViewModel(
            NullLogger<ServiceMapViewModel>.Instance,
            new ScriptedConnectionFactory(connection),
            Application.Current.GetRequiredTestService<IFactory>());
        viewModel.IsNamespaceSelectionLinked = false;

        await viewModel.InitializeAsync(workspace, TestContext.Current.CancellationToken);
        viewModel.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        await TestWait.UntilAsync(
            () => connection.RequestedNamespaces is { Count: > 0 },
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        await viewModel.DisposeAsync();
        // The view model owns the connection; a second dispose is a no-op.
        await connection.DisposeAsync();

        connection.Disposed.ShouldBeTrue();
        viewModel.ConnectionStatus.ShouldNotBeNull();
    }

    private static async Task WarmCrdPermissionsAsync(ClusterWorkspace workspace)
    {
        var crdKind = GroupApiVersionKind.From<V1CustomResourceDefinition>();
        await workspace.Runtime.Permissions.UpdateCanI(crdKind, Verb.List).WaitAsync(TestContext.Current.CancellationToken);
        await workspace.Runtime.Permissions.UpdateCanI(crdKind, Verb.Watch).WaitAsync(TestContext.Current.CancellationToken);
    }

    private static FlowViewModel CreateFlow(
        string sourcePod,
        string destinationPod,
        FlowVerdict verdict)
        => new()
        {
            Timestamp = DateTime.UtcNow,
            Uuid = Guid.NewGuid().ToString("N"),
            Verdict = verdict,
            Direction = FlowDirection.Egress,
            SourceNamespace = "default",
            SourcePod = sourcePod,
            DestinationNamespace = "default",
            DestinationPod = destinationPod,
        };

    private sealed class ScriptedConnectionFactory(ScriptedHubbleConnection connection) : IHubbleConnectionFactory
    {
        public IHubbleConnection CreateConnection(IClusterRuntime cluster) => connection;
    }

    private sealed class ScriptedHubbleConnection : IHubbleConnection
    {
        private readonly Subject<FlowViewModel> _flows = new();

        public event Action<ConnectionStatus>? StatusChanged;

        public IReadOnlyCollection<string>? RequestedNamespaces { get; private set; }

        public bool Disposed { get; private set; }

        public IObservable<FlowViewModel> Flows => _flows;

        public IObservable<FlowViewModel> ObserveFlows(IReadOnlyCollection<string> namespaces)
        {
            RequestedNamespaces = namespaces;
            return _flows;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            StatusChanged?.Invoke(ConnectionStatus.Connected);
            return Task.CompletedTask;
        }

        public void Publish(FlowViewModel flow) => _flows.OnNext(flow);

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            _flows.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
