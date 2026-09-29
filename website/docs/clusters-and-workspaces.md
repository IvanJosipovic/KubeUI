---
sidebar_position: 2
description: Connect Kubernetes clusters, switch contexts, and manage cluster workspaces in KubeUI.
---

# Clusters and workspaces

KubeUI organizes each Kubernetes connection as a cluster workspace. The navigation pane lists loaded clusters; selecting a disconnected cluster starts its connection. Once connected, KubeUI discovers resource permissions and fills in the available resource navigation.

## Load kubeconfig files

Choose **File → Load KubeConfig** and select one or more files. KubeUI adds the configured clusters to its cluster catalog. The cluster list document shows the configured cluster name and the kubeconfig source path.

If you use a non-default kubeconfig location, select that file explicitly. KubeUI does not require you to change your shell's `KUBECONFIG` environment variable.

## Connect and navigate

Select a cluster node in the navigation pane to connect. The cluster starts with its resource categories collapsed; expand it to browse the resource types available to your identity. Click a resource type to open its list as a workspace document.

You can load more than one cluster and keep their documents in the same dockable workspace. Use **Window → Reset Layout** if you want to restore the default arrangement.

## Azure Kubernetes Service

KubeUI includes an AKS import flow under **File → Import AKS Cluster**. It uses the available Azure sign-in/subscription flow to import cluster credentials into the kubeconfig catalog. The regular kubeconfig loading flow remains available for other clusters.
