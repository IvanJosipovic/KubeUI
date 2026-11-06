---
sidebar_position: 6
description: Use KubeUI workload tools to inspect logs, open consoles, forward ports, scale workloads, and manage rollouts.
---

# Workload tools

KubeUI includes focused tools for common workload operations. Which actions are available depends on the selected object, cluster capabilities, and Kubernetes permissions.

## Pod console

Where the cluster and permissions allow it, open a pod console to execute an interactive command in a container. For live log streams and controller-wide Pod logs, see [Pod logs](./pod-logs).

## Port forwarding

Use port forwarding to access a pod or service port from the local machine. Stop the forwarding session when you are done; the forwarding list shows active sessions managed by KubeUI.

## Resource operations

KubeUI exposes resource-specific actions for common workload operations:

- Scale Deployments, StatefulSets, and ReplicaSets. Bulk scaling applies one desired replica count to every selected workload.
- Pause or resume Deployments. Pausing stops new rollouts; it does not stop existing Pods.
- Inspect Deployment rollout history and undo to a selected ReplicaSet revision. Undo restores that revision's Pod template and can start a new rollout; it does not restore the old replica count.
- Suspend or resume CronJobs. Suspending stops new scheduling but does not stop Jobs that already started.
- Evict Pods through the Kubernetes eviction API. PodDisruptionBudgets and admission policies can reject an eviction; KubeUI does not bypass those rejections.
- Cordon, uncordon, and drain nodes.

Actions appear only when the connected identity has the required permissions. Scaling requires `patch` on the workload's `scale` subresource. Rollout history requires Deployment `get` and ReplicaSet `list`; undo also requires Deployment `patch`. CronJob suspend/resume requires CronJob `patch`. Pod eviction requires `create` on `pods/eviction` in each Pod namespace.

Review the target and effect before confirming an operation. Multi-resource actions report individual failures and continue with remaining resources. Secret certificate details can be inspected from the corresponding secret view.

## Metrics

Pod CPU and memory values and the cluster overview metrics require metrics data to be available from the Kubernetes cluster. KubeUI does not generate those values when the cluster has no metrics provider.
