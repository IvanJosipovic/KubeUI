# Resource List

## Current Behavior
- The list screen binds to a resource config and cluster workspace.
- DynamicData filtering, search, sorting, and row selection belong to `KubeUI.DynamicTableView`; this feature supplies Kubernetes columns and external namespace scope only.
- Selection follows the library's key-based identity mode across informer item replacement.
- Namespaced resources default to a linked namespace filter that derives selections from the workspace's selected namespaces.
- The resource list can switch to a local namespace selection mode that preserves its own filter choices without mutating the cluster workspace selection.
- The library owns the 250 ms search debounce and worker-side filtering, search, and sorting; only bound collection publication and count updates run on the UI thread.

## Validation
- Keep app tests focused on namespace scope, Kubernetes actions, and resource-specific cells. Library behavior tests live in `tests/KubeUI.DynamicTableView.Tests`.
- Keep resource actions in this view: Enter views, Delete removes, and double-tap views the selected resource set. The reusable table itself assigns no meaning to keys or tap gestures.
- Add or update list tests when changing namespace filter ownership or synchronization behavior.
- Add or update list tests when changing search timing or query application behavior.
- Use `DynamicTableViewSourceOptions.UseReplaceForUpdates` to retain the current Replace notification behavior.
