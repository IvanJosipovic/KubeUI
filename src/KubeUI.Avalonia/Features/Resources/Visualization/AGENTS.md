# Resource Visualization
## Expected View Model Logic
- Build the visualization graph through `IResourceRelationshipBuilder`; do not duplicate relationship or resource-discovery logic in the view.
- Apply the selected namespace and noise filters during graph construction. Unrelated namespaced resources must not enter the graph through incremental updates.
- Preserve valid relationships to cluster-scoped resources and explicitly related resources from other namespaces.
- Treat `RootResource` as a graph scope: include the root and resources reachable through the relationship traversal rules, then apply the root filter to both resources and relationships.
- Marshal runtime resource-change state updates and UI notifications to the UI thread. Perform relationship graph construction and other expensive work off the UI thread. Use incremental addition deltas only when the resource can affect the current graph; otherwise leave the current graph unchanged.
- Rebuild when updates can affect existing relationships, namespace selection changes, root selection changes, or visualization filters change.
- Merge incremental deltas without bypassing namespace and root filtering, and preserve pending references and seed prerequisites from the complete graph.
- Seed relationship-provider prerequisites and unresolved references through the cluster runtime. Rebuild when a required resource type is seeded so newly available relationships are reflected.
- Keep graph metadata, including pending references and required seed prerequisites, when applying type/readiness filters.
- Dispose runtime, workspace, and namespace subscriptions together with the view model; do not leave resource-change handlers active after disposal.
