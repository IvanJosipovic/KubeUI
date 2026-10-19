# Navigation Feature

## Current Behavior

- `NavigationViewModel` mirrors the cluster catalog. Adding, removing, replacing, or resetting catalog entries updates the corresponding cluster navigation nodes.
- A cluster navigation node is empty while disconnected. Its fixed cluster links are recreated when the runtime connection state changes; resource links are then populated from connected, initialized resource configurations.
- While a cluster remains connected, resource navigation is incremental:
  - resource configuration changes add, update, or remove only the affected resource link;
  - custom-resource-definition changes update the CRD branch and affected custom-resource links without rebuilding unrelated resource links;
  - cluster-name changes update navigation IDs in place.
- Resource links require `PermissionsLoaded` and the configuration's resolved `CanListAndWatch` value. Namespace-specific permission fallback is resolved by the workspace/configuration layer; this view model does not independently inspect namespace permissions.
- Typed kind behavior belongs on its `ResourceConfig<T>` subclass; CRD kind behavior, including canonical navigation placement, belongs on a matching `CustomResourceConfig` subclass. Navigation providers own sections, aliases, and feature documents only.
- `INavigationSectionProvider` owns one top-level section: it declares the section (`NavigationSection`) and, per resource kind, zero or more alias placements (`Place(IResourceConfig)`) plus section-owned documents (`INavigationDocument`). `NavigationSectionCatalog` merges built-in sections with registered providers and rejects duplicate section or document ids.
- Each `IResourceConfig` has one `NavigationPlacement Navigation` value for canonical location: section id, optional group path, and optional name/order overrides. Null section id means cluster root; CRD configs use `custom-resources`.
- A single `NavigationModelBuilder` computes the desired entries for a cluster and a single `NavigationTreeSynchronizer.Reconcile` applies them: it reuses links by id, preserves link identity, count streams, expansion, and unrelated branches, relocates moved links, and prunes empty generated sections and groups. Do not reintroduce per-category synchronizers.
- Aliases coexist with canonical links. CRD-backed links require CRD-definition list/watch access; every resource link requires resource list/watch access.
- `INavigationDocument` defines section documents (availability plus dockable creation) and replaces `NavigationFeatureDefinition`. Do not restore feature paths or a separate feature catalog.
- Port-forward navigation is recalculated only when the Pod resource configuration is processed. It is visible only when Pod list/watch permission and the port-forward create permission are available. The current implementation removes and recreates the port-forward link during that refresh.
- Resource count observables are attached only when both the matching navigation link and a seeded resource container exist. `ResourceSeeded` notifications are marshalled to the UI dispatcher; there is no retry loop for a missing link. A count stream preserves an initial zero and is retained when an existing resource link is updated.
- Connection failures update or open the shared cluster-error document. Missing namespace permission opens cluster settings and shows the permission prompt.
- One click on a navigation row activates it: resource links open their document, sections and groups toggle expansion, and disconnected clusters start connecting. Clicking an already connected cluster toggles expansion. Native TreeView double-tap expansion is suppressed so it cannot toggle a row a second time.

## Invariants

- Keep the navigation tree mutable and preserve existing resource-link instances during ordinary configuration updates whenever possible.
- Perform navigation collection mutations on the Avalonia UI thread, but do not handle resource-config events synchronously there. Resource-config events are batched off-thread, including any required configuration snapshot; the navigation update itself is then applied on the UI thread because it touches UI-owned collections and observable state.
- Do not add polling, unbounded dispatcher reposts, or full-tree rebuilds to solve ordering issues. Fix the owning lifecycle event or add a bounded, event-driven handoff.
- Preserve resource count observables and link identity when an ordinary resource configuration is updated. Structural changes such as connection transitions or required section moves may replace the relevant branch.

## Validation

- Preserve and extend the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/NavigationViewModelTests.cs`, `NavigationSectionTests.cs`, `NavigationTreeSynchronizerTests.cs`, and `LimitedAccessNavigationTests.cs`.
- For navigation changes, add or update a focused Avalonia headless regression covering the exact lifecycle or permission transition, run it before broader validation, and verify both link structure and count behavior where applicable.
- At minimum, validate connected/disconnected transitions, incremental resource-config updates, CRD updates, Pod port-forward visibility, seeded/unseeded count behavior, and missing-namespace permission handling when those areas change.
