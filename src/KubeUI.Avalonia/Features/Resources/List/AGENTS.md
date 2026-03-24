# Resource List

## Current Behavior
- The list screen binds to a resource config and cluster workspace.
- Selection is preserved across item refreshes and item replacement.
- Namespaced resources derive namespace filtering from the workspace's selected namespaces.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Features/Resources/List/`.
