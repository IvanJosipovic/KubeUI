# Cluster Features

## Ownership
- Cluster features own cluster catalog screens, workspace/session state, cluster settings, and cluster error presentation. The cluster overview stays in App because it composes Clusters and Resources capabilities; do not introduce a Clusters -> Resources reference to move it.
- Session-scoped cluster state should live in workspace or cluster services, not in views.
- `Presentation/NamespaceScopePicker` owns reusable namespace multi-select/link presentation for Resources, Helm, and Cilium. Bind the available namespace sequence and caller-owned selection list by reference; the control must not resolve a workspace, copy selections, or interpret an empty selection. Callers own linked/local behavior and readonly scope policy.
- Namespace picker labels live in Clusters resources with `NamespaceScopePicker_*` keys. Use direct Ursa control APIs here; do not add a Clusters -> Resources dependency for generated Ursa extensions.

## Validation
- Preserve existing cluster connection, permission, and CRD-driven behaviors backed by tests.
- Cluster catalog Delete is scoped to the focused list and uses the selected cluster's normal delete command and confirmation flow.
