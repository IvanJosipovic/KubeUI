# Hubble feature requirements

## Architecture

- Keep Hubble Relay transport, connection options, flow mapping, and graph data in `KubeUI.Cilium`.
- Keep navigation, document lifecycle, filtering, and Avalonia rendering in `KubeUI.Avalonia`.
- Resolve Hubble connections through `IHubbleConnectionFactory`; do not construct transport from a view or view model.
- Treat each Hubble document as the owner of its connection and subscriptions. Cancel initialization and dispose subscriptions and connections when the document closes.
- Keep Hubble documents under the Cilium navigation section, nested in the Hubble group.
- Hubble navigation is a preview feature gated by `FeatureFlagKeys.Hubble` (`kubeui.hubble`), default off. Capture the flag when the section provider is created; enabling or disabling it requires an application restart.
- Place CRDs whose API group is exactly `cilium.io` or ends in `.cilium.io` in the Cilium section, preserving their group hierarchy; do not match lookalike suffixes without the dot boundary.

## Flow and graph behavior

- Preserve individual Hubble flow events by UUID; events without UUIDs need distinct per-event cache identities.
- Keep flow-list filtering live so changing criteria also changes already received rows.
- Keep retained flow history bounded by `MaxFlows`; evict nonmatching rows before rows matching active flow and namespace filters.
- Keep service-map history bounded by both age and event count. Apply selected time window, verdict, and namespace scopes when building the graph.
- Build the service map from directed service/workload links: aggregate replica pods into their service or workload node, exclude reply flows from graph topology and counts, group remaining events by source/target link, and show destination protocol/port access points on target nodes and links regardless of observer-side traffic direction. Keep independently initiated reverse links directional; do not treat event counts as unique connections.
- Require at least one selected namespace before opening a service-map flow stream; send namespace filters to Hubble Relay and never interpret an empty selection as all namespaces.
- Keep per-flow ingestion constant-time and off the UI thread. Build graph and selected-node snapshots on background workers, cancel stale builds, and publish snapshots on the UI thread.
- Do not present unavailable Hubble fields, such as byte counts, as measured values.

## UI and tests

- Use compiled bindings and resource strings for visible UI text. Keep pointer and keyboard interactions out of view code-behind.
- The flow list namespace selector must support multiple namespaces, link to cluster namespace selection by default, and allow local selection while unlinked.
- Keep service-map graph construction in the Cilium layer and render it with GraphX inside a custom Avalonia control.
- Service Map graph nodes must be draggable and edges must show target-only traffic-direction arrows. Provide a selector for GraphX built-in layout algorithms; use no edge routing by default. Use `CompoundFDP` by default with namespace parent vertices and one `World` parent for namespace-less external nodes; exclude synthetic parents from rendering. Preserve compound membership in the source graph and through the external layout path, since GraphX's built-in factory graph copy drops parent-child associations. Other selected GraphX layouts may operate on a flat copy while retaining source graph membership. Do not calculate node positions manually. Re-run layout after layout selector changes and preserve manually moved node positions during flow-count-only updates.
- Rendered service-map nodes expose a localized right-click `View` action that opens a non-document properties tool in the right dock. Show details available on the aggregated `ServiceMapNode`, including its identity, workload, flow counts, and access points; do not imply the aggregate is a live Kubernetes resource.
- Match Resource Visualization's shared icon/kind/name identity content in the service map. Keep type, name, and namespace evenly spaced, then add a larger gap before connection counts and incoming destination ports; keep the same details in node tooltips/properties and directed connection details on edge tooltips. Show generic globe icons for external endpoints, never Kubernetes resource icons for DNS or World endpoints.
- Add focused tests for flow mapping and identity, filter changes, time and namespace scoping, graph aggregation, retention, DI registration, and document disposal.
- Follow `src/KubeUI.Avalonia/AGENTS.md` for Avalonia UI and headless testing rules.
