# Resource Visualization
## Presentation
- Resource Visualization and Hubble service-map nodes share `Infrastructure.Presentation.ResourceGraphNodeContent`: centered 128×128 icon/kind/name stack with 64px icons. Keep feature-specific details outside shared node content; preserve them in each feature's tooltips and properties views.

## Expected View Model Logic
- Build the visualization graph through `IResourceRelationshipBuilder`; do not duplicate relationship or resource-discovery logic in the view.
- Apply the selected namespace and noise filters during graph construction. Unrelated namespaced resources must not enter the graph through incremental updates.
- Preserve valid relationships to cluster-scoped resources and explicitly related resources from other namespaces.
- Treat `RootResource` as a graph scope: include the root and resources reachable through the relationship traversal rules, then apply the root filter to both resources and relationships.
- Marshal runtime resource-change state updates and UI notifications to the UI thread. Perform relationship graph construction and other expensive work off the UI thread. Use incremental addition deltas only when the resource can affect the current graph; otherwise leave the current graph unchanged. For named scopes, a resource can affect the graph only when it matches the selector, belongs to the scope namespace, or is already connected through relationships or owner references.
- Rebuild when updates can affect existing relationships, namespace selection changes, root selection changes, or visualization filters change.
- Merge incremental deltas without bypassing namespace and root filtering, and preserve pending references and seed prerequisites from the complete graph.
- Seed relationship-provider prerequisites and unresolved references through the cluster runtime. Rebuild when a required resource type is seeded so newly available relationships are reflected.
- Keep graph metadata, including pending references and required seed prerequisites, when applying type/readiness filters.
- Dispose runtime, workspace, and namespace subscriptions together with the view model; do not leave resource-change handlers active after disposal.
- Named live scopes use `VisualizationScope`: namespace, localized resource description, annotation selector, and an explicit namespace-readonly option. The visualization must not know about Helm release names or ownership annotation keys; callers construct these selectors. Combine annotation requirements with AND semantics and select matching resources as roots, retaining their relationship ancestry, descendants, and cluster-scoped matches.
- Apply selector projection after both full builds and incremental merges. Rebuild when selector membership changes to rediscover or remove whole subgraphs. Initialize a local namespace selection using the runtime namespace object when available, without changing cluster selection. Disable both namespace selection and linking only when the scope requests readonly controls. Generic resource and template-preview initialization remain unchanged and clear named scope state.
- For every visualization scope, do not cancel an in-flight full build for each informer event. Update the resource store, publish the current valid snapshot, coalesce changes received during construction, and then build the latest snapshot so busy clusters cannot starve graph publication. Scope/filter changes, initialization, and disposal must still invalidate obsolete work immediately. Preserve incremental processing when no full rebuild is active.

## Pipeline stage ownership
- `VisualizationResourceStore` owns keyed resource snapshots and owner-reference indexing; the ViewModel must not maintain a second resource index.
- `VisualizationBuildCoordinator` owns pending rebuild coalescing, cancellation, and latest-request-wins version checks.
- `ResourceGraphProjection` owns root, annotation-selector, namespace, and incremental namespace reachability and must preserve graph metadata.
- `ResourceGraphDisplayFilter` owns display-only type/readiness filtering and must not mutate the complete graph.
- `VisualizationSeedPlanner` owns unresolved-reference and seed-prerequisite resolution; seed planning must remain pure and separate from informer/resource mutation.
- `VisualizationPipelineState` is the immutable boundary published from background preparation to the UI thread; stale versions must not publish it.
- `ResourceGraphControl` consumes immutable display graphs and owns GraphX conversion, diffing, attachment, and layout scheduling. Refit viewport as resources load until user adjusts zoom or pan; preserve adjusted viewport for subsequent updates.
