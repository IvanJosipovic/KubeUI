# Kubernetes Tests

- Use the production Kubernetes client with `FakeKubernetesHttpApi` for local deterministic tests.
- Shared scenario tests use `KubernetesScenarioHarnessFactory` and the same assertions against fake and Kind backends; Kind is enabled with `KUBEUI_RUN_KIND_TESTS=1`.
- Add backend data to one test method with `KubernetesBackendData.Enabled`; do not create separate Fake and Kind test methods.
- Keep REST behavior in the fake `DelegatingHandler` and WebSocket-only behavior in the WebSocket transport fake.
- Do not introduce test implementations of `IClusterRuntime`, `IKubernetes`, or `TestClusterRuntime`-style wrappers.
- Await informer/resource completion and pass `TestContext.Current.CancellationToken`; no `Task.Delay` or `Thread.Sleep` in test code.
- Mark backend-matrix suites with `[Trait("Category", "Kind")]` so CI can filter cluster-backed coverage while the environment variable controls whether Kind cases are materialized.
