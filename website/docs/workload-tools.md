---
sidebar_position: 6
---

# Workload tools

KubeUI includes focused tools for common workload operations. Which actions are available depends on the selected object, cluster capabilities, and Kubernetes permissions.

## Pod logs and console

Open a pod's logs to inspect container output. Where the cluster and permissions allow it, open the pod console to execute an interactive command in a container.

## Port forwarding

Use port forwarding to access a pod or service port from the local machine. Stop the forwarding session when you are done; the forwarding list shows active sessions managed by KubeUI.

## Resource operations

KubeUI exposes resource-specific actions such as cordon, uncordon, and drain for nodes. Review the target and the effect before confirming an operation. Secret certificate details can be inspected from the corresponding secret view.

## Metrics

Pod CPU and memory values and the cluster overview metrics require metrics data to be available from the Kubernetes cluster. KubeUI does not generate those values when the cluster has no metrics provider.
