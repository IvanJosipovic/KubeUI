# Architecture remaining-work backlog

**Status:** Current prioritized ownership backlog. Project ownership and allowed dependencies are documented in [ownership-map.md](ownership-map.md).

Keep the existing project split. Do not add paired Core/UI projects, a universal contracts assembly, or extract controls without demonstrated reuse. Work in order: finish boundary checks and registration cleanup first; complete resource-document creation and startup separation next; begin higher-risk workflow extractions only after those smaller changes land.

## Priorities

1. **Feature-owned view registration — low risk.** Each feature service-registration extension exposes its own view factories. App composes features and supplies application adapters; shell does not enumerate feature views or repeat feature registrations. Done when a new feature view resolves without a shell registration edit and DI/view-resolution tests pass.
2. **Resources-owned document factory — medium risk.** Move generic resource ViewModel creation behind a Resources-owned factory. Use generated registrations for known model types and a static `GenericKubernetesObject` path for dynamically discovered custom resources. App retains tab reuse, activation, IDs and docking. Done when typed and custom-resource documents preserve behavior and full-trim Release execution.
3. **App startup responsibilities — low–medium risk.** Extract cluster-error document presentation and idempotent shutdown/telemetry coordination. Keep framework lifecycle hooks and event attach/detach in App. Done when tests cover handler cleanup, repeated shutdown requests, and reuse of the cluster-error document.
4. **Pod log stream-session lifecycle — completed.** `PodLogStreamSession` owns stream opening/reading, reconnect attempts, cancellation, and reader disposal. `PodLogsViewModel` retains source selection and UI presentation. Existing view-free workflow tests pass; full-trim Release publish succeeds with existing linker diagnostics.
5. **Visualization incremental updates — completed.** `VisualizationIncrementalUpdateCoordinator` owns incremental admission, pending-change coalescing, addition-delta construction, and merge/projection. `VisualizationViewModel` supplies scope state and decides when a merge requires a full rebuild. Visualization workflow tests pass; full-trim Release publish succeeds with existing linker diagnostics.
6. **Workspace permissions and overview metrics — completed.** `PermissionRefreshCoordinator` owns trigger coalescing, debounce, follow-up scheduling, and the in-progress guard while `ClusterWorkspace` remains the public facade and supplies the refresh pass. The overview gauges consume an immutable `ClusterMetricSnapshot` from `ClusterMetricSnapshotBuilder`. Focused tests cover coalescing, follow-up passes, disposal, and metric values/units.
7. **Boundary enforcement and documentation — start early.** Keep automated checks for forbidden project references, dependency cycles, and feature references from Controls/Avalonia foundation. Preserve justified acyclic feature dependencies. Maintain this ownership map and backlog as the current architecture documents.

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
