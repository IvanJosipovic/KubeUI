# Navigation Feature

## Current Behavior
- The navigation tree reflects the current cluster catalog.
- Cluster resource nodes populate only after cluster connect and workspace initialization complete.
- Connection failures open or update a shared cluster error document.
- Missing Namespace Resource list/watch permission opens cluster settings and shows a prompt.
- Selecting a resource navigation link should keep the badge count visible, including `0` when the opened runtime list is empty.
- Cluster navigation uses a long-lived mutable tree:
  - Initial cluster population may clear and seed a node from scratch when the cluster node itself is created or first initialized.
  - After that, lifecycle sync should preserve the existing tree and upsert only the required base/manual items instead of doing a full snapshot rebuild.
  - Per-resource permission updates should add, update, or remove only that single resource entry.
  - Custom resource definitions may still batch their incoming change notifications, but CRD navigation mutations should use the same incremental add/update/remove primitives as regular resources.
- Category nodes must be idempotent:
  - Repeated lifecycle sync and resource updates must reuse the same category node by id.
  - If duplicate category nodes are encountered, merge their children and collapse them back to a single node.
- Port-forward navigation is coupled to the Pod resource permission flow:
  - Only Pod permission completion should trigger a port-forward visibility refresh.
  - Port-forward should not be re-evaluated on unrelated resource permission updates.
  - Port-forward should use the same category/item upsert primitives as the rest of navigation so unrelated sync paths do not remove or duplicate it.
- When deciding whether a resource nav item is visible:
  - Require permissions to be loaded for the resource config.
  - Prefer the config's resolved `CanListAndWatch` state.
  - For namespaced resources, still allow visibility when namespace-scoped list/watch is available in a known workspace namespace even if the aggregate config flag is false.
  - Known namespaces come from the runtime namespace list after workspace initialization materializes any settings-backed fallback namespaces.

## Validation
- Preserve the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/`.
- Add or update navigation tests when changing incremental resource updates, port-forward visibility timing, or resource badge count behavior.
