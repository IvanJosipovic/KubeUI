# Cluster Workspace

## Current Behavior
- Workspace state wraps an `IClusterRuntime` and exposes cluster-scoped UI state.
- Resource configs are not initialized eagerly when a workspace is created.
- CRD add, update, and delete events update workspace resource configs, GVK catalog entries, and seeded informers.
- A CRD `Modified` event reprocesses the resource configuration only when `metadata.generation` changes; metadata-only and status-only updates are ignored. Events without a generation fall back to reprocessing.
- A CRD `Deleted` event removes matching custom-resource configurations even when the tombstone spec cannot resolve a served version. Match by definition name first, then by spec group and kind.
- Disconnect clears queued CRD revisions and observed generations so reconnect relist events re-register configurations.
- If the cluster cannot list namespaces globally, materialize settings-backed fallback namespaces into the runtime namespace set.
- Use that namespace preparation from both workspace initialization and namespaced resource seeding.
- If the cluster cannot list/watch a namespaced resource globally, the runtime should create informers for every known namespace where that resource has both list and watch access.
- Informer creation for namespaced resources remains in the runtime and uses the namespaces currently known to the workspace/runtime.
- Namespaced seeding must wait for API discovery when the resource scope is not known yet; never treat an incomplete discovery list as a cluster-scoped resource and return with zero informers.
- Workspace connect/init should refresh resource permissions before any UI command or navigation logic depends on cached `CanI(...)` results.
- Authorization uses one `SelfSubjectRulesReview` per namespace and evaluates cached rules locally. Cluster-scope checks and incomplete, failed, or erroring rules reviews fall back to `SelfSubjectAccessReview`.
- Rules-review evaluation must match the API server's RBAC semantics: a rule constrained by `resourceNames` authorizes no request that KubeUI evaluates, because KubeUI evaluates without an object name, and only exact resource matches and the `*/subresource` form are accepted.
- Rule-derived answers are not written to the permission index. The namespace rule set is the cache for namespace-scoped decisions; a cached denial must never outlive the rule set that produced it.
- Permission invalidation is trigger-only: connect, namespace-set changes, optional RBAC list/watch events, manual refresh from cluster settings, and scoped `403 Forbidden` responses. Do not add polling, periodic refresh timers, or window-activation refreshes.
- A `403 Forbidden` response invalidates the permission cache at most once per burst, and never for a self-subject rules review. Reporting every denial would wipe the cache, re-run every review, and reproduce the same denial in a loop.
- Coalesce concurrent refresh triggers without dropping them: a trigger received while a refresh worker is active schedules one follow-up refresh after the current pass. The shared worker debounces trigger bursts for 500 ms and combines full and namespace-only refresh scopes, with full refresh taking precedence.
- Namespace-set changes discard only namespace-scoped authorization decisions; retain cluster-wide decisions so an added or removed namespace does not trigger redundant access reviews for globally allowed resources.
- Start RBAC watchers only for kinds whose resource configuration has both list and watch access. Without those rights, permissions remain best-effort until another trigger fires.
- Events exist so downstream UI can update incrementally.
- `Alias`/`DisplayName`/`IsEnabled` are user preferences applied and persisted by `ClusterWorkspacePreferences`, attached by `ClusterWorkspaceCatalog` before the workspace is added to the catalog.
- Raise `ResourceConfigProcessed` per config as soon as its permission evaluation completes; do not batch at the source, so ready resources are not blocked by unrelated slow permission checks.
- Custom resource configs are dropped on disconnect and re-registered when the CRD informer lists resources again.
- The workspace remains responsible for materializing settings-backed namespaces before permission-dependent UI relies on namespace-scoped access checks.

## Constraints
- Keep cluster-wide state and runtime subscriptions here instead of in individual views.
- Keep permission-refresh orchestration here; navigation should react to workspace events rather than owning permission polling or bootstrap sequencing.
