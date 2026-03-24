# Cluster Workspace

## Current Behavior
- Workspace state wraps an `IClusterRuntime` and exposes cluster-scoped UI state.
- Resource configs are not initialized eagerly when a workspace is created.
- CRD add, update, and delete events update workspace resource configs, model cache entries, and seeded informers.

## Constraints
- Keep cluster-wide state and runtime subscriptions here instead of in individual views.
