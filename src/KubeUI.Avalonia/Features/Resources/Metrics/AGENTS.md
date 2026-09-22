# Metrics feature guidance

- Keep metrics views in C# Avalonia code using compiled bindings and fluent declarative composition.
- Keep metrics lifecycle, cancellation, cache invalidation, chart disposal, and selected-tab state scoped to the owning control or workspace.
- Do not add XAML event handlers, reflection, mutable static timers, or direct view-to-runtime calls from event handlers.
- User-facing labels and status text belong in `Assets/Resources.resx` and must be accessed through generated resources.
- Metrics Server and Prometheus are both supported backends; provider fallback and unavailable/error states must remain visible and testable.
- UI tests must use rendered controls, `TestClusterGenerator`, observable predicates, task gates, or `TestWait`; do not use `Task.Delay` or `Thread.Sleep`.
