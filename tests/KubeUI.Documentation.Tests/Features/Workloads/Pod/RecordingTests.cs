using System.Net;
using System.Text;
using Avalonia.Controls;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Documentation.Tests.Infra;
using KubeUI.Testing.Kubernetes.Bootstrap;

namespace KubeUI.Documentation.Tests.Features.Workloads.Pod;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    public Task Follow_pod_logs_up_the_controller_chain_video(string theme)
    {
        var webPodLogLines = Array.Empty<string>();
        var featuredPodLogLine = $"{WalkthroughDemoResources.FeaturedPodName}: health check passed";
        return KubeUIWalkthrough.Create(theme, "pod-logs")
            .Intro(
                "Follow pod logs",
                "Watch a Pod's live logs, then follow its ReplicaSet and Deployment to include every child Pod.")
            .FakeCluster("demo-cluster", config =>
            {
                webPodLogLines = ConfigureLogHandler(config);
            })
            .LoadView<MainView, MainViewModel>(
                x => x.ViewModel.Initialize(),
                connectToCluster: false)
            .Speak("I'll open Pods to find the application workload.")
            .OpenCluster("demo-cluster")
            .Speak(
                "The cluster is ready. I'll open its Pods.",
                root => KubeUIWalkthroughRecorder.HasNavigationItem(root, "Pods"))
            .SelectNavigation("Pods")
            .Speak("I'll right-click the web Pod and open a new logs view.")
            .SelectPodLogs(
                WalkthroughDemoResources.FeaturedPodName,
                root => root.GetVisualDescendants().OfType<PodLogsView>()
                    .Any(view => view.IsVisible && view.ViewModel.Object?.Kind == V1Pod.KubeKind))
            .Speak(
                "Pod logs arrive as a live stream. Move up to the owning ReplicaSet.",
                root => HasLogsForScope(
                    root,
                    V1Pod.KubeKind,
                    1,
                    [featuredPodLogLine],
                    webPodLogLines.Where(line => line != featuredPodLogLine).ToArray()))
            .JumpToLogController(V1ReplicaSet.KubeKind)
            .Speak(
                "The active ReplicaSet owns these Pods, so their streams appear together. Move up to Deployment scope.",
                root => HasLogsForScope(
                    root,
                    V1ReplicaSet.KubeKind,
                    webPodLogLines.Length,
                    webPodLogLines))
            .JumpToLogController(V1Deployment.KubeKind)
            .Speak(
                "The Deployment owns this ReplicaSet, so its child Pods stay in scope. If a Pod restarts or is replaced, the view reconnects to its new log stream.",
                root => HasLogsForScope(
                    root,
                    V1Deployment.KubeKind,
                    webPodLogLines.Length,
                    webPodLogLines))
            .RecordAsync();
    }

    private static string[] ConfigureLogHandler(TestClusterConfig config)
    {
        var featuredPod = config.InitialResources.OfType<V1Pod>()
            .Single(pod => pod.Metadata?.Name == WalkthroughDemoResources.FeaturedPodName);
        var replicaSetOwner = featuredPod.Metadata!.OwnerReferences!
            .Single(owner => owner.Kind == V1ReplicaSet.KubeKind);
        var podLogLines = config.InitialResources.OfType<V1Pod>()
            .Where(pod => pod.Metadata?.OwnerReferences?.Any(owner => owner.Uid == replicaSetOwner.Uid) == true)
            .Select(pod => $"{pod.Metadata!.Name}: health check passed")
            .ToArray();
        config.HttpHandlerFactory = static () => [new PodLogsRecordingHandler()];
        return podLogLines;
    }

    private static bool HasLogsForScope(
        Control root,
        string resourceKind,
        int expectedPodCount,
        IReadOnlyList<string> expectedLines,
        IReadOnlyList<string>? unexpectedLines = null)
    {
        var logsView = root.GetVisualDescendants().OfType<PodLogsView>()
            .FirstOrDefault(view => view.IsVisible && view.ViewModel.Object?.Kind == resourceKind);
        if (logsView is null)
        {
            return false;
        }

        var viewModel = logsView.ViewModel;
        if (!string.IsNullOrWhiteSpace(viewModel.ConnectionError))
        {
            throw new InvalidOperationException($"Pod log stream failed: {viewModel.ConnectionError}");
        }

        var scopePodCount = viewModel.ScopeItems.Count == 1
            ? viewModel.ScopeItems[0].ResolvedPodCount
            : 0;
        return scopePodCount == expectedPodCount
            && expectedLines.All(line => viewModel.Logs.Text.Contains(line, StringComparison.Ordinal))
            && (unexpectedLines is null
                || unexpectedLines.All(line => !viewModel.Logs.Text.Contains(line, StringComparison.Ordinal)));
    }

    private sealed class PodLogsRecordingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (request.Method == HttpMethod.Get
                && requestPath.EndsWith("/log", StringComparison.Ordinal))
            {
                var podName = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries)[^2];
                var payload = Encoding.UTF8.GetBytes(
                    $"{podName}: application container started\n"
                    + $"{podName}: accepted request GET /healthz\n"
                    + $"{podName}: health check passed\n");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(payload, writable: false)),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
