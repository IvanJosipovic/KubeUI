# Navigation Feature

## Current Behavior

- `NavigationViewModel` mirrors the cluster catalog. Adding, removing, replacing, or resetting catalog entries updates the corresponding cluster navigation nodes.
- A cluster navigation node is empty while disconnected. Its fixed cluster links are recreated when the runtime connection state changes; resource links are then populated from connected, initialized resource configurations.
- While a cluster remains connected, each reconcile pass applies the current workspace state incrementally by id:
  - configuration changes add, update, or remove only the links whose ids are affected;
  - custom-resource-definition changes update the CRD branch and affected custom-resource links while unrelated links keep their instances;
  - cluster-name changes update navigation IDs in place.
- Canonical resource-link ids use the resource group and kind, not the served API version, so CRD storage-version changes keep the same link instance and count stream.
- Alias-link ids include the placement section, an alias slot (`crd` or the provider section id), and the resource group and kind. Different alias sources in the same section cannot collide, and provider group-path moves keep the alias instance.
- Subscribing an already-populated workspace performs one reconcile for the whole workspace; do not reconcile once per existing resource configuration.
- Resource links require `PermissionsLoaded` and the configuration's resolved `CanListAndWatch` value. Namespace-specific permission fallback is resolved by the workspace/configuration layer; this view model does not independently inspect namespace permissions.
- Typed kind behavior belongs on its `ResourceConfig<T>` subclass; CRD kind behavior, including canonical navigation placement, belongs on a matching `CustomResourceConfig` subclass. Navigation providers own sections, aliases, and feature documents only.
- `INavigationSectionProvider` owns one top-level section: it declares the section (`NavigationSection`) and, per resource kind, zero or more alias placements (`Place(IResourceConfig)`) plus section-owned documents (`NavigationDocument`). `NavigationSectionCatalog` merges built-in sections with registered providers and rejects duplicate section or document ids.
- Each `IResourceConfig` has one `NavigationPlacement Navigation` value for canonical location: section id, optional group path, and optional name/order overrides. Null section id means cluster root.
- A single `NavigationTreeBuilder.Reconcile` computes and applies the desired tree for a cluster: it reuses links by id, preserves link identity, count streams, expansion, and unrelated branches, relocates moved links, and prunes empty generated sections and groups. Do not reintroduce separate entry models, id helpers, or per-category synchronizers.
- The builder is shared by every cluster and holds no per-reconcile state. Desired ids and the existing-item index live in a per-reconcile context so concurrent clusters cannot observe each other's state.
- Custom resources are placed at their canonical `Navigation` location and, unless that already resolves to the same `custom-resources` section and group path, additionally under `custom-resources`. Aliases coexist with canonical links. CRD-backed links require CRD-definition list/watch access; every resource link requires resource list/watch access.
- `NavigationDocument` defines section documents (availability plus dockable creation). Do not restore feature paths or a separate feature catalog.
- CRD resource configurations are not retained across disconnect: `ResetWorkspaceState` drops them and the CRD informer re-registers them on reconnect. Do not reintroduce descriptor retention or replay.
- Port-forward navigation is recalculated on every reconcile: the link is present while Pod list/watch permission and the port-forward create permission are available, and its existing instance is reused otherwise. A missing or denied Pod configuration removes it.
- Resource count observables are attached only when both the matching navigation link and a seeded resource container exist. `ResourceSeeded` notifications are marshalled to the UI dispatcher; there is no retry loop for a missing link. A count stream preserves an initial zero and is retained when an existing resource link is updated with the same kind; a kind change clears the old stream and rebinds it to the replacement kind's container.
- Connection failures update or open the shared cluster-error document. Missing namespace permission opens cluster settings and shows the permission prompt.
- One click on a navigation row activates it: resource links open their document, sections and groups toggle expansion, and disconnected clusters start connecting. Clicking an already connected cluster toggles expansion. Keyboard Enter or Space activates the focused row. Native TreeView double-tap expansion is suppressed so it cannot toggle a row a second time.

## Invariants

- Keep the navigation tree mutable and preserve existing resource-link instances during ordinary configuration updates whenever possible.
- Perform navigation collection mutations on the Avalonia UI thread. Resource-config events received on the UI thread are applied inline; events from other threads coalesce through at most one posted reconcile per cluster, and an event that arrives after that reconcile starts schedules the next one.
- Do not add polling, unbounded dispatcher reposts, or full-tree rebuilds to solve ordering issues. Fix the owning lifecycle event or add a bounded, event-driven handoff.
- Preserve resource count observables and link identity when an ordinary resource configuration is updated. Structural changes such as connection transitions or required section moves may replace the relevant branch.

## Validation

- Preserve and extend the behaviors covered by `tests/KubeUI.Avalonia.Tests/Shell/Navigation/NavigationViewModelTests.cs`, `NavigationSectionTests.cs`, `NavigationTreeBuilderTests.cs`, and `LimitedAccessNavigationTests.cs`.
- For navigation changes, add or update a focused Avalonia headless regression covering the exact lifecycle or permission transition, run it before broader validation, and verify both link structure and count behavior where applicable.
- At minimum, validate connected/disconnected transitions, incremental resource-config updates, CRD updates, Pod port-forward visibility, seeded/unseeded count behavior, and missing-namespace permission handling when those areas change.
