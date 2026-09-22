# Navigation Feature

## Current Behavior

- `NavigationViewModel` mirrors the cluster catalog. Adding, removing, replacing, or resetting catalog entries updates the corresponding cluster navigation nodes.
- A cluster navigation node is empty while disconnected. Its fixed cluster links are recreated when the runtime connection state changes; resource links are then populated from connected, initialized resource configurations.
- While a cluster remains connected, resource navigation is incremental:
  - resource configuration changes add, update, or remove only the affected resource link;
  - custom-resource-definition changes update the CRD branch and affected custom-resource links without rebuilding unrelated resource links;
  - cluster-name changes update navigation IDs in place.
- Resource links require `PermissionsLoaded` and the configuration's resolved `CanListAndWatch` value. Namespace-specific permission fallback is resolved by the workspace/configuration layer; this view model does not independently inspect namespace permissions.
- Navigation categories are reused by their generated IDs and empty generated categories are removed. Duplicate categories are not generally merged, so new code must not create duplicate category nodes.
- Port-forward navigation is recalculated only when the Pod resource configuration is processed. It is visible only when Pod list/watch permission and the port-forward create permission are available. The current implementation removes and recreates the port-forward link during that refresh.
- Resource count observables are attached only when both the matching navigation link and a seeded resource container exist. `ResourceSeeded` notifications are marshalled to the UI dispatcher; there is no retry loop for a missing link. A count stream preserves an initial zero and is retained when an existing resource link is updated.
- Connection failures update or open the shared cluster-error document. Missing namespace permission opens cluster settings and shows the permission prompt.
- Selecting a resource link opens its document; selecting a category toggles its expansion. Selecting a disconnected cluster starts connection asynchronously, and selecting an already connected cluster toggles expansion.

## Invariants

- Keep the navigation tree mutable and preserve existing resource-link instances during ordinary configuration updates whenever possible.
- Perform navigation collection mutations on the Avalonia UI thread, but do not handle resource-config events synchronously there. Resource-config events are batched off-thread, including any required configuration snapshot; the navigation update itself is then applied on the UI thread because it touches UI-owned collections and observable state.
- Do not add polling, unbounded dispatcher reposts, or full-tree rebuilds to solve ordering issues. Fix the owning lifecycle event or add a bounded, event-driven handoff.
- Preserve resource count observables and link identity when an ordinary resource configuration is updated. Structural changes such as connection transitions or required category moves may replace the relevant branch.

## Validation

- Preserve and extend the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/NavigationViewModelTests.cs` and `LimitedAccessNavigationTests.cs`.
- For navigation changes, add or update a focused Avalonia headless regression covering the exact lifecycle or permission transition, run it before broader validation, and verify both link structure and count behavior where applicable.
- At minimum, validate connected/disconnected transitions, incremental resource-config updates, CRD updates, Pod port-forward visibility, seeded/unseeded count behavior, and missing-namespace permission handling when those areas change.
