# Avalonia Tests

## Structure
- Keep tests near the feature or resource behavior they verify.
- Keep shared Avalonia test setup in `Infra/`.
- Use `KubernetesScenarioClusterScope` for Kubernetes-backed tests. It creates the production `KubeUI.Kubernetes.Cluster` with the real Kubernetes client and fake HTTP/WebSocket transport.
- Use `KubernetesTestWorkspaceScope` when the test owns both a workspace and its scenario harness.
- Do not add test-only implementations of `IClusterRuntime`, wrappers around it, or mocks of `IKubernetes`. Configure the shared transport/harness for the scenario instead.
- Use `CreateDisconnected` only for connection-lifecycle behavior; use the connected scope for resource, informer, authorization, and navigation tests.

## Coverage Focus
- Prefer behavior tests for view models, resource configs, and shared UI behaviors that are easy to drift.
- Preserve tests when migrating them. If a test asserted an implementation detail through a wrapper, replace it with an observable behavior assertion or add the missing transport scenario; do not silently delete coverage.
- Kubernetes-client parity is exercised by the shared fake/Kind matrix. Avalonia tests that exercise HTTP-backed authorization, informer, or limited-access behavior use `AvaloniaTheory` with `[KubernetesBackendData]`; deterministic UI-only and fault-injection tests continue to use the fake transport. Local runs use fake only and CI enables Kind with `KUBEUI_RUN_KIND_TESTS=1`. Matrix rows are displayed with `- fake` or `- kind`.
- Prefer event/informer completion predicates over fixed sleeps. If polling is unavoidable, centralize it in a cancellable wait helper with a bounded timeout and include the final predicate assertion.
- Use `KubeUI.Testing.TestWait`/observable predicates for polling. Tests must not use `Task.Delay` or `Thread.Sleep`; synchronous callback-order tests should use explicit task-completion gates so the intended interleaving is deterministic.
- Async test helpers accept and honor a `CancellationToken`; use `TestContext.Current.CancellationToken` when invoking cancellable APIs and pass cancellation through to HTTP, informer, and wait operations wherever the API supports it.
- Bound every task wait and awaited async command with `TestContext.Current.CancellationToken`; do not leave `Task.WaitAsync`, socket waits, or command execution uncancellable.

## Attributes
Tests using Avalonia capabilities should use `[AvaloniaFact]`. Pure model, serialization, transport, and service tests should use `[Fact]`/`[Theory]`. Do not use `[AvaloniaFact]` merely because a test is in an Avalonia test assembly.

## Commands and cleanup

- Always include `--hangdump --hangdump-timeout 1m` when running `dotnet test`.
- Await async commands and preserve their exception/cancellation behavior.
- Dispose scenario scopes with `await using`; dispose view models/workspaces owned directly by a test.
- Keep fake transport behavior in `KubeUI.Testing`, not in individual tests.
- Avalonia UI tests remain focused on UI behavior; cluster-backed REST/watch/auth parity belongs in the Kubernetes test project's backend matrix. WebSocket URI and local socket mechanics use deterministic transport/session fakes because they test client wiring or local ownership, not cluster state.
- When replacing a test double, preserve the original observable behavior or strengthen it with a production-runtime regression; do not remove coverage merely to make a test fit the shared harness.
- Use coverage results to target high-fan-out navigation, resource readiness, relationship, CRD, and transport branches with behavior assertions.
- In Release or parallel CI runs, wait for the exact view/cache state after resource mutations and selection changes; `Dispatcher.UIThread.RunJobs()` only drains queued UI work and is not a data-readiness guarantee.
