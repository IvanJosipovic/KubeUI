---
sidebar_position: 6
description: Use KubeUI workload tools to inspect logs, open consoles, forward ports, and manage Kubernetes workloads.
---

# Workload tools

KubeUI includes focused tools for common workload operations. Which actions are available depends on the selected object, cluster capabilities, and Kubernetes permissions.

## Pod console

Where the cluster and permissions allow it, open a pod console to execute an interactive command in a container. For live log streams and controller-wide Pod logs, see [Pod logs](./pod-logs).

## Port forwarding

Use port forwarding to access a pod or service port from the local machine. Stop the forwarding session when you are done; the forwarding list shows active sessions managed by KubeUI.

## Resource operations

KubeUI exposes resource-specific actions such as cordon, uncordon, and drain for nodes. Review the target and the effect before confirming an operation. Secret certificate details can be inspected from the corresponding secret view.

## Metrics

Pod CPU and memory values and the cluster overview metrics require metrics data to be available from the Kubernetes cluster. KubeUI does not generate those values when the cluster has no metrics provider.
