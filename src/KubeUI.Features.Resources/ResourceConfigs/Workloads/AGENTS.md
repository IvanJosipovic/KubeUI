# Workload Resources

## Subtree Guidance
- Workload resource kinds own their own columns and workload-specific menu actions.
- Pod-specific logs, console, metrics, and port-forwarding behavior stay local to `v1/Pod/`.
- Deployment, StatefulSet, and ReplicaSet scaling must use each kind's Kubernetes `scale` subresource and check its namespaced `patch` permission.
- Deployment rollout history must resolve ReplicaSets by controller owner UID. Undo restores the selected Pod template without changing unrelated Deployment fields or replica count.
- CronJob suspend/resume patches only `spec.suspend`; Pod eviction must use `policy/v1` eviction and never fall back to direct deletion.
- Bulk actions must confirm impact, apply only to eligible selected resources, report per-resource failures, and continue after individual failures.
