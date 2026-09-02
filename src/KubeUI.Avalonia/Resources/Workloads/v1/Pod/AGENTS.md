# Pod Resource

## Current Behavior
- Pods expose custom list cells for containers, CPU, memory, and status.
- Pod actions include logs, console, and port forwarding flows.
- Pod-specific supporting views and view models stay local to this folder.

## Pod Logs
- A pod-scoped log view uses the title `Pod Logs` and selects all containers when launched from the pod's View Logs action.
- Present sources in one compact Ursa TreeComboBox in the toolbar: Resource -> Pod -> Container.
- Keep the popup hierarchy expanded, show partial parent selection, and let parent checks apply to descendants without closing the popup.
- Treat the source tree as the canonical Pod/container selection state. Do not add parallel selector collections or projection synchronization.
- Reconcile stable resource, Pod, and container nodes by identity so checked state and bound node instances survive topology refreshes.
- Unchecking a resource removes it from the session. Pod and container checks directly define the effective streams, including an intentional zero-stream selection.
- A controller-scoped log view uses the title `{Kind} Logs` and selects all descendant pods and all containers by default.
- Controller navigation advances exactly one owner level at a time, such as `Pod -> ReplicaSet -> Deployment` and `Pod -> Job -> CronJob`.
- Allow controller navigation to resolved custom-resource owners as well as built-in workload controllers, and display the actual parent kind.
- Cluster-scoped parent resources resolve descendant pods across namespaces so navigating to them keeps pod and container log selection functional.
- Scope changes clear output from the previous scope and reconnect without mixing log entries.
- Pod and container selection changes clear errors from the previous connection immediately; failures from the new selection remain visible.
- Resource-name prefixes default to enabled when entering a multi-pod or multi-container display mode, while explicit user changes survive reconnects in the same mode.
- Label the action that resumes automatic scrolling as `Follow Logs`; keep it visible and disable it while the editor is already following the newest output.
- Recompute Follow Logs availability when either the editor scroll offset or viewport changes, including when resizing introduces vertical overflow.
- Use the down-arrow-to-line icon for Follow Logs so it reads as returning to the bottom of the output.
- Keep the log action toolbar compact without visual separators between action groups.
- Use a broom icon for Clear Logs so the action does not imply deleting a Kubernetes resource or file.
- Loading an optional parent controller must not prevent otherwise authorized pod logs from opening.
- Tests must cover cold-cache workload launches, scope presentation, owner navigation, topology changes, connection failures, and reconnect behavior.
- Multi-resource logs accept up to 20 selected Pods or controllers, deduplicate overlapping resolved Pods, warn above 25 Pod/container streams, and refuse to connect above 100 streams.
- Keep one-resource launches behavior-compatible. Resource nodes expose resolution status and resolved Pod count; removing the final resource produces an intentional empty logs state.
- Resource Lists expose one View Logs action. When a compatible logs tool is active for the cluster, its submenu explicitly offers Open New Logs View or Add to Current Logs View; never assume the user wants selections grouped.
- Combined exports use a multi-resource filename and manifest, and describe cross-stream output as arrival-ordered.
- New sessions and resource-add refreshes open all selected Pod/container streams concurrently, load the last 500 lines from each, and then follow live output.
