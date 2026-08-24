# Shared Kubernetes Test Infrastructure

Shared helpers are organized under `Kubernetes/Bootstrap`, `Kubernetes/Scenarios`, `Kubernetes/Transport`, and `Kubernetes/Infrastructure`; cross-cutting polling helpers live under `Utilities`.

- Build scenarios through the production `KubeUI.Kubernetes.Cluster` and `k8s.Kubernetes` client.
- Use `TestClusterGenerator` with `TestClusterConfig.Fake()` or `TestClusterConfig.Kind()` to create the production runtime and real Kubernetes client. `FakeKubernetesHttpApi` remains the local deterministic transport.
- Keep `TestCluster` limited to the generated client/runtime, direct fixture CRUD, and cleanup. Permission discovery, access evaluation, and informer seeding must run through production APIs from the test or application flow.
- Fake clients with different identities must share one API backing state so resources and watches have the same cross-client visibility as Kind.
- Keep backend-independent assertions in shared assertion helpers and create clusters directly through `TestClusterGenerator`. Backend selection is controlled by `KUBEUI_RUN_KIND_TESTS=1`.
- Configure typed initial resources, permissions, latency, connection failure, and disconnected startup directly on `TestClusterConfig`. Transport conditions are implemented with delegating handlers and work with both Fake and Kind backends.
- Do not add test implementations of `IClusterRuntime`, `IKubernetes`, or application-owned workspace classes.
- Model WebSocket-only operations with the transport fake; do not introduce an HTTP listener or arbitrary socket server for ordinary REST requests.
- Wait for informer/resource state or explicit task completion, pass `TestContext.Current.CancellationToken`, and never use `Task.Delay` or `Thread.Sleep` in tests.
- Dispose `TestCluster` when initialization succeeds; generator failure paths must clean up the generated client/cluster. Test and transport APIs must accept and forward cancellation tokens; use bounded, predicate-based waits for readiness and cleanup.
- `SeedResource`/CRUD completion and informer observation are separate states; expose or await explicit predicates rather than treating a dispatcher flush as synchronization.
- Kind-backed suites create isolated Docker clusters per test; run them
