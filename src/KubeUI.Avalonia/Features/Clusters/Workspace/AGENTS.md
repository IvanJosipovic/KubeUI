# Cluster Workspace

## Current Behavior
- Workspace state wraps an `IClusterRuntime` and exposes cluster-scoped UI state.
- Resource configs are not initialized eagerly when a workspace is created.
- CRD add, update, and delete events update workspace resource configs, model cache entries, and seeded informers.
- If the cluster cannot list namespaces globally, materialize settings-backed fallback namespaces into the runtime namespace set.
- Use that namespace preparation from both workspace initialization and namespaced resource seeding.
- If the cluster cannot list/watch a namespaced resource globally, the runtime should create informers for every known namespace where that resource has both list and watch access.
- Informer creation for namespaced resources remains in the runtime and uses the namespaces currently known to the workspace/runtime.
- Workspace connect/init should refresh resource permissions before any UI command or navigation logic depends on cached `CanI(...)` results.
- Each resource config permission refresh should publish a per-resource completion signal so downstream UI can update incrementally.
- The workspace remains responsible for materializing settings-backed namespaces before permission-dependent UI relies on namespace-scoped access checks.
- `ClusterWorkspaceViewModel.Connect()` should only initialize workspace state after the runtime reports `Connected == true`, because runtime connect may swallow its own failures and return without throwing.

## Constraints
- Keep cluster-wide state and runtime subscriptions here instead of in individual views.
- Keep permission-refresh orchestration here; navigation should react to workspace events rather than owning permission polling or bootstrap sequencing.
