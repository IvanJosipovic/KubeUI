# Pod Resource

## Current Behavior
- Pods expose custom list cells for containers, CPU, memory, and status.
- Pod actions include logs, console, and port forwarding flows.
- Pod-specific supporting views and view models stay local to this folder.

## Pod Logs
- A pod-scoped log view uses the title `Pod Logs`, displays the pod name, hides the pod selector, and selects all containers when launched from the pod's View Logs action.
- The first toolbar row presents the resource name and namespace as standard labeled values in a compact 24px row; its Name field has a subtle 4px offset beyond the resource-list toolbar inset.
- Pod and container selectors use the same compact 20px height, 2px trailing margin, and clear-button styling as the resource-list namespace selector.
- A controller-scoped log view uses the title `{Kind} Logs`, shows the pod selector on the second toolbar row, and selects all descendant pods and all containers by default.
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
