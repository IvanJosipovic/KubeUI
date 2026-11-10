# Plan: Unify Dynamic Resource Registration on API Discovery

**Status:** Complete — discovery-backed registration, permission-safe seeding, refresh UI, optional CRD printer-column enrichment, and guidance are implemented.  
**Scope:** Replace CRD-object-driven registration of dynamic resource kinds with reconciliation from Kubernetes API discovery.

## Goal

Use the authenticated `/api` and `/apis` discovery snapshots as the single source of truth for served dynamic API resources. A user must not need cluster-wide `list`/`watch` access to `customresourcedefinitions` just to discover a resource kind.

The CRD informer remains useful when authorized, but only as a **refresh signal**: CRD add, relevant update, or delete triggers a fresh `/apis` snapshot. It must not directly register or remove resource configs. CRD-triggered refreshes use a 350 ms trailing debounce; connect and manual refreshes stay immediate.

## Current state

- `KubernetesApiDiscoveryClient` reads `/api` and `/apis`, uses ETags, coalesces refreshes, and publishes only complete snapshots.
- `Cluster` refreshes on connect, CRD add/delete/spec-generation events, and manual request. CRD events only trigger discovery and coalesce through one 350 ms trailing-debounce worker; connect and manual requests bypass the delay.
- `ClusterWorkspace` reconciles dynamic configs from snapshots and keeps permission evaluation independent from advertised verbs.
- Namespaced dynamic seeding uses known namespaces with list and watch access when cluster-wide access is unavailable.
- OpenAPI schemas remain independent and best-effort for YAML completion.
- Discovered custom-resource lists always use ordinary JSON object lists. When the CRD informer is authorized and seeded, its matching served-version printer columns supplement the discovery-backed config.

## Target flow

```text
connect / CRD event / manual refresh / stale-resource error
                         ↓
              authenticated API discovery
                         ↓
             complete discovery snapshot
                         ↓
     reconcile dynamic resource configs by GVK
                         ↓
          existing RBAC checks and informers
                         ↓
                navigation and lists
```

### Discovery and reconciliation — implemented

1. Normalize API discovery entries into immutable descriptors: API group, served version, kind, plural resource name, scope, and advertised endpoint verbs. Exclude subresources (for example, `widgets/status`), incomplete entries, and kinds already owned by typed built-in configs.
2. Publish a snapshot/event from `IClusterRuntime` after a successful, complete grouped-API discovery refresh. A `304 Not Modified` reuses the last complete snapshot. A failed or partial `/apis` refresh must not be treated as an empty snapshot or prune registrations.
3. Have `ClusterWorkspace` reconcile that snapshot. Select a specialized `CustomResourceConfig` using its existing group/kind/optional-version identity, or use the generic fallback. Register, update, and remove configs from snapshot differences only.
4. Track dynamic origin through `ResourceOrigin`; keep `IsCustomResource` computed. Do not introduce a second user-maintained kind/definition registry.
5. On disconnect, clear dynamic configs and model-catalog entries. Re-register them from the next successful snapshot.

### CRD events and refresh policy — implemented

- Keep the existing CRD informer when the user has cluster-scoped CRD list/watch permission.
- CRD add/delete and spec-generation changes request a discovery refresh. Metadata/status-only changes do not.
- Debounce CRD-only triggers by 350 ms in one worker. Triggers during the quiet window restart it; triggers during a refresh schedule one follow-up after the quiet window. Connect, manual, and seeding refreshes remain immediate.
- Make one runtime-owned refresh trigger path. Avoid duplicate refresh calls from both the informer callback and workspace event handling.
- If CRD list/watch is unavailable, refresh on connect, explicit user request, and a stale-resource response such as a 404. Do not add periodic polling in this plan. Without polling or CRD watch permission, changes may wait until the next trigger.
- Cluster settings provide a **Refresh API discovery** action for an on-demand path.

### Authorization and seeding — implemented

- API discovery describes server capabilities, not the connected identity's RBAC. Keep current permission evaluation; never infer `list`/`watch` access from discovery verbs.
- Only expose a usable resource list and start its informer when existing list/watch authorization checks pass.
- For namespaced dynamic resources, use workspace-materialized fallback namespaces and create informers only in namespaces where both list and watch are allowed. Do not send a cluster-wide list request when only namespace-scoped access exists.
- If a user has only `get` on named objects and no `list`, discovery can identify the kind but cannot populate a resource table. Direct get-by-name is out of scope.

### Printer columns and YAML schema — implemented

- `/api` and `/apis` discovery metadata does not contain CRD `additionalPrinterColumns`.
- Dynamic configs always start with Name, Namespace when namespaced, and Age columns. If CRD list/watch access is available, use the CRD informer cache to add `additionalPrinterColumns` from the exact served version selected by discovery.
- Compile each CRD column's JSONPath against the full generic resource object, so ordinary watch updates keep printer-column values current. Keep `Definition` null and keep the discovery-selected GVK; CRD data supplements presentation only.
- If the CRD informer is unavailable or the matching definition/version is missing, retain the generic columns. CRD events trigger discovery refresh; they never register or remove dynamic kinds.
- Keep OpenAPI schema loading independent and best-effort. Missing schema must not block browsing; YAML completion remains available when the schema loads.

## Required design decision: API version — deterministic policy adopted

Current CRD configuration selects the storage version. `/apis` lists served versions and may provide a preferred version, but does not identify the CRD storage version in the aggregated discovery snapshot used by this implementation.

**Decision:** current freshness wins, followed by stable, beta, alpha, and numeric version ordering. This is deterministic but does not identify storage version. Gateway API guidance now reflects that limitation. If preserving storage-version behavior is mandatory, pure `/apis` registration is insufficient without another source for storage-version metadata.

## Implementation phases

1. **Discovery contract:** complete.
2. **Single registration pipeline:** complete.
3. **Permission-safe informers:** complete; fake-backend regression covers CRD-denied access and namespace-scoped informers.
4. **Printer columns:** complete. Discovery remains the kind/version source; the optional CRD informer supplies JSONPath columns for the selected served version, with generic columns when CRD access is unavailable.
5. **UI and docs:** refresh action and architecture guidance complete; user documentation updated for restricted-RBAC discovery.

## Acceptance tests

- A cluster identity can list/watch a custom resource but cannot list/watch CRDs: discovery still registers the kind and shows only authorized objects.
- A discovered kind without user list/watch access does not start an informer or bypass RBAC.
- Namespaced resources use only authorized known namespaces; cluster-scoped resources use cluster-scoped permission checks.
- CRD events trigger discovery refresh but do not directly change the config catalog.
- A burst of CRD events coalesces into one discovery refresh; connect and manual refreshes are not debounced.
- A successful snapshot adds, updates, and removes dynamic kinds. A failed or partial snapshot preserves the last complete registration.
- Built-in kinds do not duplicate; subresources are excluded; disconnect/reconnect clears and rebuilds dynamic entries.
- Readable CRDs supplement discovery-backed configs with printer columns for the selected served version; denied CRD access leaves discovery registration and generic columns intact. Printer values continue updating from full-object watch events.
- Version selection is deterministic and covered for multi-version CRDs and Gateway API configs.

## Out of scope

- Periodic discovery polling or real-time discovery push (Kubernetes discovery has no watch stream).
- A user-maintained manual GVK/CRD registry.
- Listing objects when the identity has only `get` and no `list` permission.
- Expanding access to built-in resources such as StorageClasses when the identity lacks their required permissions.
