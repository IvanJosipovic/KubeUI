# Cluster Settings

## Current Behavior
- Cluster settings are initialized from a selected cluster workspace.
- This screen is the current destination when namespace list permission is missing.
- Metrics service and Prometheus provider settings are rendered through C# Avalonia views.
- Metrics settings changes persist through `ISettingsService` and update conditional Prometheus fields.
- The active metrics backend is displayed separately from configured settings and updates from runtime notifications.

## Validation
- Keep rendered metrics selectors and conditional Prometheus fields covered by Avalonia headless tests.
- Cover active backend status rendering and runtime updates when changing metrics lifecycle state.
- Test settings through controls and bindings, not only direct model mutation.

## Lifecycle
- `ClusterSettingsViewModel` implements `IDisposable` and unsubscribes from the cluster runtime, the cluster settings, and the metrics settings. The view disposes the view model in `OnDetachedFromVisualTree`, so a closed settings document does not keep the view model alive.
- View models that subscribe to runtime or settings events must expose a disposal path; subscribing without an unsubscribe leaks the view model for the lifetime of the process.
