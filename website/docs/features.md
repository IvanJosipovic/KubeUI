---
sidebar_position: 1
description: Explore KubeUI features for browsing Kubernetes resources, editing YAML, visualizing relationships, and managing workloads.
---

# Feature overview

KubeUI is a desktop workspace for everyday Kubernetes exploration and operations.

## Cluster workspaces

- Load Kubernetes configuration files and connect to multiple clusters.
- Open resource views in a dockable workspace that supports multiple windows and monitors.
- Browse built-in resources and custom resources discovered from CRDs.

## Resource exploration

- Inspect resources in sortable, searchable, filterable tables.
- Select namespaces and navigate resources available to your identity.
- Visualize references between Kubernetes objects.

## YAML editing

- Inspect and edit Kubernetes resources as YAML.
- Use Kubernetes-aware completion and field documentation when schemas are available.
- Review validation feedback and run a server-side dry run before saving.

## Workload and operations tools

- View pod logs and open a pod console where permitted.
- Forward pod and service ports.
- View pod CPU and memory metrics when the cluster provides metrics.
- Cordon, uncordon, or drain nodes.
- Inspect certificates represented in Kubernetes secrets.

## Access-aware workflows

KubeUI adapts resource navigation to the permissions available to the connected identity, including reduced-access and namespace-scoped environments.

Follow the task guides for [clusters](./clusters-and-workspaces), [resource browsing](./resource-browsing), [YAML editing](./yaml-editor), [visualization](./resource-visualization), [workload tools](./workload-tools), and [Pod logs](./pod-logs).
