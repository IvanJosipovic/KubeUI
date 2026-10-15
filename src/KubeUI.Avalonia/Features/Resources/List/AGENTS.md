# Resource List

## Current Behavior
- The list screen binds to a resource config and cluster workspace.
- DynamicData filtering, search, sorting, and row selection belong to `SvcSystems.Avalonia.DynamicTableView`; this feature supplies Kubernetes columns and external namespace scope only.
- Selection follows the library's key-based identity mode across informer item replacement.
- Resource selection follows the view model across tab hides and Dock view recreation by resource key; returning to a resource tab restores selected resources that remain visible through current filters.
- Namespaced resources default to a linked namespace filter that derives selections from the workspace's selected namespaces.
- The resource list can switch to a local namespace selection mode that preserves its own filter choices without mutating the cluster workspace selection.
- The library owns the 250 ms search debounce and worker-side filtering, search, and sorting; only bound collection publication and count updates run on the UI thread.
- Selecting a row only selects it. The Properties view opens explicitly (double-tap, Enter, or the "View" action). While a Properties view is open (docked, or auto-hidden in the right strip), a single-item selection change shows that item in it (`ResourceConfigBase.ShowInOpenProperties`) without opening, expanding or focusing it; selecting a different resource kind replaces it in place (same dock and position, or still auto-hidden). Multi-selection leaves it unchanged. Restoring selection on tab activation follows the same rule.

## Validation
- Keep app tests focused on namespace scope, Kubernetes actions, and resource-specific cells.
- Keep resource actions in this view: Enter views, Delete removes, and double-tap views the selected resource set. The reusable table itself assigns no meaning to keys or tap gestures.
- Add or update list tests when changing namespace filter ownership or synchronization behavior.
- Add or update list tests when changing search timing or query application behavior.
