# Kubernetes Tests

- Use the production Kubernetes client with `FakeKubernetesHttpApi` for local deterministic tests.
- Shared backend tests use `TestClusterGenerator` and the same assertions against fake and Kind backends; Kind is enabled with `KUBEUI_RUN_KIND_TESTS=1`.
- Add `[KubernetesBackendData]` to one test method; do not create separate Fake and Kind test methods. Rows are displayed with the test method name followed by `- fake` or `- kind`.
- HTTP-backed authorization and limited-access behavior must use the shared backend matrix too; configure `TestClusterConfig` and the generated client directly rather than constructing test doubles.
- Keep REST behavior in the fake `DelegatingHandler` and WebSocket-only behavior in the WebSocket transport fake.
- Do not introduce test implementations of `IClusterRuntime`, `IKubernetes`, or `TestClusterRuntime`-style wrappers.
- Await informer/resource completion and pass `TestContext.Current.CancellationToken`; no `Task.Delay` or `Thread.Sleep` in test code.
- A successful Kubernetes API call is not proof that the informer cache has observed the event; wait for the exact resource/list predicate before asserting cache state.
- Mark backend-matrix suites with `[Trait("Category", "Kind")]` so CI can filter cluster-backed coverage while the environment variable controls whether Kind cases are materialized.
- Preserve behavior when consolidating suites: replace duplicate backend methods with one theory, but keep every meaningful CRUD, watch, authorization, CRD, and namespace-seeding assertion.
- Add focused regressions for high-fan-out production paths and meaningful branch behavior; do not inflate coverage with implementation-only assertions.
