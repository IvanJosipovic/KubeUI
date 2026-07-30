# Shared Kubernetes Test Infrastructure

- Build scenarios through the production `KubeUI.Kubernetes.Cluster` and `k8s.Kubernetes` client.
- Use `FakeKubernetesHttpApi` as the local deterministic transport and `KindClusterScenarioHarness` for the real-cluster backend.
- Keep backend-independent assertions in shared scenario assertion helpers and create harnesses through `KubernetesScenarioHarnessFactory`. Backend selection is controlled by `KUBEUI_RUN_KIND_TESTS=1`.
- Do not add test implementations of `IClusterRuntime`, `IKubernetes`, or application-owned workspace classes.
- Model WebSocket-only operations with the transport fake; do not introduce an HTTP listener or arbitrary socket server for ordinary REST requests.
- Wait for informer/resource state or explicit task completion, pass `TestContext.Current.CancellationToken`, and never use `Task.Delay` or `Thread.Sleep` in tests.
- Dispose a harness when initialization fails; the shared factory owns that failure cleanup.
