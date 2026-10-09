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
