# Architecture remaining-work backlog

**Status:** Architecture restructure backlog; all listed implementation items are complete. Project ownership and allowed dependencies are documented in [ownership-map.md](ownership-map.md), and shared-control policy is in [shared-ui-components.md](shared-ui-components.md).

Keep the existing project split. Do not add paired Core/UI projects, a universal contracts assembly, or extract controls without demonstrated reuse. Remaining work is maintenance: keep the boundaries and ownership docs aligned when future changes land.

## Priorities

1. **Feature-owned view registration — complete.** Feature service-registration extensions register their own generated view factories; App composes feature registration and discovers only App-owned views. `ViewLocatorTests` verify feature registration and generated resolution.
2. **Resources-owned document factory — complete.** `ResourceListDocumentFactory` creates typed lists through static creators and uses a static `GenericKubernetesObject` path for runtime-discovered kinds. App retains tab reuse, activation, IDs, and docking. Navigation tests cover typed list reuse and custom-resource documents; the documented trimmed Release validation passed during the restructure.
3. **App startup responsibilities — complete.** App owns cluster-error presentation and idempotent shutdown/telemetry coordination while keeping framework lifecycle hooks in App. `ClusterErrorDocumentPresenterTests`, `ApplicationShutdownTests`, and navigation tests cover presentation reuse and repeated shutdown.
4. **Pod log stream-session lifecycle — completed.** `PodLogStreamSession` owns stream opening/reading, reconnect attempts, cancellation, and reader disposal. `PodLogsViewModel` retains source selection and UI presentation. Existing view-free workflow tests pass; full-trim Release publish succeeds with existing linker diagnostics.
5. **Visualization incremental updates — completed.** `VisualizationIncrementalUpdateCoordinator` owns incremental admission, pending-change coalescing, addition-delta construction, and merge/projection. `VisualizationViewModel` supplies scope state and decides when a merge requires a full rebuild. Visualization workflow tests pass; full-trim Release publish succeeds with existing linker diagnostics.
6. **Workspace permissions and overview metrics — completed.** `PermissionRefreshCoordinator` owns trigger coalescing, debounce, follow-up scheduling, and the in-progress guard while `ClusterWorkspace` remains the public facade and supplies the refresh pass. The overview gauges consume an immutable `ClusterMetricSnapshot` from `ClusterMetricSnapshotBuilder`. Focused tests cover coalescing, follow-up passes, disposal, and metric values/units.
7. **Boundary enforcement and documentation — complete; maintain ongoing.** `ProjectBoundaryTests` enforce allowed references, acyclicity, and markup-extension generator ownership. `ownership-map.md` and `shared-ui-components.md` record current ownership and promotion rules.
8. **Resource property-section reuse — complete.** `ResourcePropertiesSection` owns the repeated expandable StackPanel composition across resource-kind views; each kind retains its localized header and property contents. Direct `ItemsControl`/data-display sections remain kind-owned.

## Validation gates

- After each discrete change, run `dotnet build KubeUI.slnx --tl:off -clp:ErrorsOnly`, then the narrowest focused tests. Require a nonzero executed-test count.
- Cross-project changes also require a full-trim Desktop Release publish. Review linker diagnostics; use static registration, source generation or correct annotations. Do not suppress trim warnings project-wide.
- Workflow extractions need deterministic tests without Views. Do not count partial-file splits as extraction.
- Update the closest `AGENTS.md` when ownership or workflow invariants move.

## Explicitly out of scope

- Paired Core/UI projects or a universal contracts assembly.
- Wholesale control extraction based only on similar-looking UI.
- Moving the cluster overview into Clusters; it composes Clusters and Resources and would create a dependency cycle.
- Global-using cleanup as a substitute for project encapsulation.
