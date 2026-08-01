# Shared Kubernetes Test Infrastructure

Shared helpers are organized under `Kubernetes/Scenarios`, `Kubernetes/Transport`, and `Kubernetes/Infrastructure`; cross-cutting polling helpers live under `Utilities`.

- Build scenarios through the production `KubeUI.Kubernetes.Cluster` and `k8s.Kubernetes` client.
- Use `FakeKubernetesHttpApi` as the local deterministic transport and `KindClusterScenarioHarness` for the real-cluster backend.
- Keep scenario harnesses limited to cluster/transport provisioning, direct fixture CRUD, credentials, and cleanup. Permission discovery, access evaluation, and informer seeding must run through production APIs from the scenario or application flow.
- Fake clients with different identities must share one API backing state so resources and watches have the same cross-client visibility as Kind.
- Keep backend-independent assertions in shared scenario assertion helpers and create harnesses through `KubernetesScenarioHarnessFactory`. Backend selection is controlled by `KUBEUI_RUN_KIND_TESTS=1`.
- Use `KubernetesScenarioClusterScope` for cluster-only scenario tests; keep application-owned workspace scopes in the owning test project.
- Do not add test implementations of `IClusterRuntime`, `IKubernetes`, or application-owned workspace classes.
- Model WebSocket-only operations with the transport fake; do not introduce an HTTP listener or arbitrary socket server for ordinary REST requests.
- Wait for informer/resource state or explicit task completion, pass `TestContext.Current.CancellationToken`, and never use `Task.Delay` or `Thread.Sleep` in tests.
- Dispose a harness when initialization fails; the shared factory owns that failure cleanup.
- Harness APIs and process/transport helpers must accept and forward cancellation tokens; use bounded, predicate-based waits for readiness and cleanup.
- `SeedResource`/CRUD completion and informer observation are separate states; expose or await explicit predicates rather than treating a dispatcher flush as synchronization.
- Kind-backed suites create isolated Docker clusters per test; run them
