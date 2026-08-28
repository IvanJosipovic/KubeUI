# Pod Resource

## Current Behavior
- Pods expose custom list cells for containers, CPU, memory, and status.
- Pod actions include logs, console, and port forwarding flows.
- Pod-specific supporting views and view models stay local to this folder.

## Pod Logs
- A pod-scoped log view uses the title `Pod Logs`, displays the pod name, hides the pod selector, and selects all containers when launched from the pod's View Logs action.
- The first toolbar row uses a compact identity strip with an icon, prominent resource name, and a muted namespace chip when the resource is namespaced.
- A controller-scoped log view uses the title `{Kind} Logs`, shows the pod selector on the second toolbar row, and selects all descendant pods and all containers by default.
- Controller navigation advances exactly one owner level at a time, such as `Pod -> ReplicaSet -> Deployment` and `Pod -> Job -> CronJob`.
- Allow controller navigation to resolved custom-resource owners as well as built-in workload controllers, and display the actual parent kind.
- Scope changes clear output from the previous scope and reconnect without mixing log entries.
- Pod and container selection changes clear errors from the previous connection immediately; failures from the new selection remain visible.
- Resource-name prefixes default to enabled when entering a multi-pod or multi-container display mode, while explicit user changes survive reconnects in the same mode.
- Hide the Jump to Present action while the editor is already pinned to the newest log output.
- Loading an optional parent controller must not prevent otherwise authorized pod logs from opening.
- Tests must cover cold-cache workload launches, scope presentation, owner navigation, topology changes, connection failures, and reconnect behavior.
