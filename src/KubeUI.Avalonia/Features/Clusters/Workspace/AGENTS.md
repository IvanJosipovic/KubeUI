# Cluster Workspace

## Current Behavior
- Workspace state wraps an `IClusterRuntime` and exposes cluster-scoped UI state.
- Resource configs are not initialized eagerly when a workspace is created.
- CRD add, update, and delete events update workspace resource configs, model cache entries, and seeded informers.
- If the cluster cannot list namespaces globally, materialize settings-backed fallback namespaces into the runtime namespace set.
- Use that namespace preparation from both workspace initialization and namespaced resource seeding.
- If the cluster cannot list/watch a namespaced resource globally, the runtime should create informers for every known namespace where that resource has both list and watch access.
- Informer creation for namespaced resources remains in the runtime and uses the namespaces currently known to the workspace/runtime.

## Constraints
- Keep cluster-wide state and runtime subscriptions here instead of in individual views.
