using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class HubbleLiveFlowViewDiagnosticTests(ITestOutputHelper output) : LiveClusterTestBase
{
    protected override string LiveClusterName => "microk8s";

    [LiveClusterFact]
    public async Task Flow_view_connects_and_receives_live_microk8s_flows()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = Application.Current.GetTestServices();
        using var logProvider = new TestOutputLoggerProvider(output);
        services.GetRequiredService<ILoggerFactory>().AddProvider(logProvider);

        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, LiveCluster.Cluster);
        await workspace.Connect().WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
        var crdConfig = workspace.GetResourceConfig<V1CustomResourceDefinition>(
            GroupApiVersionKind.From<V1CustomResourceDefinition>());
        output.WriteLine(
            "Workspace state: connected={0}; status={1}; lastError={2}; CRD permissionsLoaded={3}; CRD listWatch={4}",
            workspace.Runtime.Connected,
            workspace.Runtime.Status,
            workspace.Runtime.LastError,
            crdConfig.PermissionsLoaded,
            crdConfig.CanListAndWatch);
        await TestWait.UntilAsync(
            () => workspace.Runtime.IsCiliumInstalled(),
            timeoutMs: 30_000,
            cancellationToken: cancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await using var viewModel = services.GetRequiredService<FlowListViewModel>();
        var view = services.GetRequiredService<FlowListView>();
        view.DataContext = viewModel;

        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(cancellationToken);

        await viewModel.InitializeAsync(workspace, cancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

        try
        {
            await TestWait.UntilAsync(
                () => (viewModel.IsConnected && viewModel.FlowCount > 0)
                    || viewModel.ConnectionStatus.StartsWith("Error", StringComparison.OrdinalIgnoreCase),
                timeoutMs: 30_000,
                cancellationToken: cancellationToken,
                beforePoll: () => Dispatcher.UIThread.RunJobs());
        }
        catch (TimeoutException exception)
        {
            throw new TimeoutException(
                $"Hubble view did not receive flows. Connected: {viewModel.IsConnected}; "
                + $"flow count: {viewModel.FlowCount}; status: {viewModel.ConnectionStatus}.",
                exception);
        }

        output.WriteLine(
            "Flow view state: connected={0}; flowCount={1}; connectionStatus={2}; windowVisible={3}; viewVisible={4}",
            viewModel.IsConnected,
            viewModel.FlowCount,
            viewModel.ConnectionStatus,
            window.IsVisible,
            view.IsVisible);

        viewModel.ConnectionStatus.StartsWith("Error", StringComparison.OrdinalIgnoreCase).ShouldBeFalse(
            $"Hubble connection failed with status: {viewModel.ConnectionStatus}.");
        viewModel.FlowCount.ShouldBeGreaterThan(0);
        viewModel.IsConnected.ShouldBeTrue();
        window.IsVisible.ShouldBeTrue();
        window.Content.ShouldBe(view);
    }

    private sealed class TestOutputLoggerProvider(ITestOutputHelper output) : ILoggerProvider
    {
        private ITestOutputHelper? _output = output;

        public ILogger CreateLogger(string categoryName) => new TestOutputLogger(this, categoryName);

        public void Dispose() => Interlocked.Exchange(ref _output, null);

        private void Write<TState>(
            string categoryName,
            LogLevel logLevel,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var testOutput = Volatile.Read(ref _output);
            if (testOutput is null)
            {
                return;
            }

            testOutput.WriteLine(
                "{0}: {1}: {2}{3}",
                logLevel,
                categoryName,
                formatter(state, exception),
                exception is null ? string.Empty : Environment.NewLine + exception);
        }

        private sealed class TestOutputLogger(TestOutputLoggerProvider provider, string categoryName) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                {
                    provider.Write(categoryName, logLevel, state, exception, formatter);
                }
            }
        }
    }
}
