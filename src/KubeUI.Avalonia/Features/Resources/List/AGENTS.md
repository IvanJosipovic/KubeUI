# Resource List

## Current Behavior
- The list screen binds to a resource config and cluster workspace.
- Selection is preserved across item refreshes and item replacement.
- Namespaced resources default to a linked namespace filter that derives selections from the workspace's selected namespaces.
- The resource list can switch to a local namespace selection mode that preserves its own filter choices without mutating the cluster workspace selection.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Features/Resources/List/`.
- Add or update list tests when changing namespace filter ownership or synchronization behavior.
