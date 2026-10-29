---
name: kubeui-live-cluster-debugging
description: Debug KubeUI behavior that depends on real Kubernetes cluster state or live API/stream responses when fake or deterministic tests do not reproduce the issue.
---

# KubeUI live-cluster debugging

Use this workflow when a bug depends on real cluster resources, permissions, endpoint discovery, port forwarding, or live streams, and local fake tests pass or lack the required data. Keep the live test narrow and temporary unless the user asks to retain it.

## Set up a gated live test

- Read `tests/KubeUI.Avalonia.Tests/AGENTS.md` and `.agents/skills/kube-ui-testing/SKILL.md` first. Follow their build-before-test, cancellation, disposal, and Microsoft.Testing.Platform rules.
- Inspect `LiveClusterFactAttribute` and `LiveClusterTestBase` in the current checkout. The attribute only gates execution using `KUBEUI_LIVE_CONTEXT`; the base fixture connects using its `LiveClusterName`. Set both to the intended kubeconfig context.
- Set `KUBEUI_LIVE_CONTEXT` only for the test process. Never change the user's persistent environment or current kubeconfig context.
- Preserve staged and unrelated work. Check `git status --short` before editing. Do not stage or revert user changes.
- Build first with `dotnet build KubeUI.slnx --tl:off -clp:ErrorsOnly`. If a running process locks normal output, identify it and use a task-specific `BaseOutputPath`; do not kill an unrelated process.

For an Avalonia view, prefer the real app test services and view model over a mock:

1. Resolve services with `Application.Current.GetTestServices()`.
2. Create `ClusterWorkspace` using `LiveCluster.Cluster`, then await `workspace.Connect()`.
3. Workspace connection starts configured informers but may return before a needed cache contains its first list. Wait for the specific resource or feature predicate with `TestWait.UntilAsync`; pump `Dispatcher.UIThread.RunJobs()` when the predicate depends on UI work. Do not use sleeps.
4. Resolve the production view model and view from test services, set `DataContext`, show a test window, and call the view model's normal initialization method with the live workspace.
5. Bound every wait and pass `TestContext.Current.CancellationToken`. Dispose the workspace, view model, window, and any owned logger provider.
6. Capture observable state on failure: connection status, connected state, row/flow count, visibility, and relevant runtime status. If the UI reduces an exception to a generic status, capture the existing `ILogger` output from the test boundary. Prefer a debugger when available; do not add lasting production logging just for a diagnostic run.

Run only the selected test first, with `--minimum-expected-tests 1`, and verify the runner reports a nonzero test count. Set the context for that invocation, for example in PowerShell:

```powershell
$env:KUBEUI_LIVE_CONTEXT = 'microk8s'
dotnet test --project tests/KubeUI.Avalonia.Tests/KubeUI.Avalonia.Tests.csproj --no-restore --filter-method <fully-qualified-method> --minimum-expected-tests 1
```

## Separate startup, cache, and transport failures

- A feature status such as `Cilium not detected` can mean the required CRD informer cache is empty, even when the live cluster has Cilium. Confirm workspace connection, list/watch access, and cached CRD data before diagnosing Hubble transport.
- A connected view with zero rows points to stream delivery or filtering. A generic `Error` may hide a stream exception; inspect captured logs and transport state.
- For Service forwarding, inspect live Service `spec.ports` and the matching EndpointSlice (`kubernetes.io/service-name`, ports, ready/serving endpoint, target Pod). Service `port` can differ from EndpointSlice port. Example observed here: Service port `80`, named `targetPort: grpc`, EndpointSlice port `4245`. Code that compares only `80 == 4245` rejects a valid endpoint and surfaces later as an HTTP/2 handshake failure.
- Use read-only commands such as `kubectl --context <context> get service ... -o yaml`, `get endpointslice ... -o yaml`, and `get pods ...`; use the same named context as the test. Do not apply, patch, restart, or delete live resources unless separately requested.
- Compare the failing app path with a standard client path when useful. A successful `kubectl port-forward` can confirm the Service and Relay work while narrowing failure to KubeUI's forwarding bridge or mapping.

## Turn live evidence into a fix

- Keep the LiveCluster test focused on the user-visible path and useful diagnostics. Do not make ordinary fake tests depend on MicroK8s.
- Convert the discovered rule into a deterministic regression at the narrowest useful seam (for example, Service-to-EndpointSlice port mapping). Confirm it fails before the fix, then passes after.
- Rebuild before rerunning tests. Rerun the regression, the live test, then the nearest relevant test class when practical.
- Remove temporary production instrumentation. Keep or remove a diagnostic LiveCluster test according to the user's request; if removing it, delete only files created for this investigation. Check final status to ensure unrelated staged work remains untouched.
