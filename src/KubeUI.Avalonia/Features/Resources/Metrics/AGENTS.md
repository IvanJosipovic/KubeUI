# Metrics feature guidance

- Keep metrics views in C# Avalonia code using compiled bindings and fluent declarative composition.
- Keep metrics lifecycle, cancellation, cache invalidation, chart disposal, and selected-tab state scoped to the owning control or workspace.
- Do not add XAML event handlers, reflection, mutable static timers, or direct view-to-runtime calls from event handlers.
- User-facing labels and status text belong in `Assets/Resources.resx` and must be accessed through generated resources.
- Metrics Server and Prometheus are both supported backends; provider fallback and unavailable/error states must remain visible and testable.
- Kubernetes Metrics Server charts aggregate cached PodMetrics for namespaces and selector-based workloads (Deployment, StatefulSet, DaemonSet, ReplicaSet, and Job); PVC and Ingress metrics remain unsupported by this backend.
- Resource-list history cells share the generic `MetricsHistoryCellBase<TResource>` visuals, bucketing, cancellation, refresh, and tooltip behavior; keep resource-specific sample selection, Prometheus filters, and limits in resource adapters.
- Azure Managed Prometheus uses Azure CLI authentication and a selected Azure Monitor workspace. Never persist access tokens; obtain fresh credentials for query requests.
- UI tests must use rendered controls, `TestClusterGenerator`, observable predicates, task gates, or `TestWait`; do not use `Task.Delay` or `Thread.Sleep`.
